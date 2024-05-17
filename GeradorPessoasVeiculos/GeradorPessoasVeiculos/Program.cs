using GeradorPessoasVeiculos;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine();
        Console.WriteLine("|||||||||||| GERADOR DE PESSOAS |||||||||||||");
        Console.WriteLine();
        Console.WriteLine("|||||||| Digite SAIR para finalizar |||||||||");

        while (true)
        {
            int quantidade = await SolicitaQuantidade();
            await GerarPessoa(quantidade);
        }
    }


    static async Task<int> SolicitaQuantidade()
    {
        Console.WriteLine();
        Console.Write("Digite o número de pessoas a serem geradas: ");
        string quantidadeInput = Console.ReadLine();
        int quantidade;

        while (!int.TryParse(quantidadeInput, out quantidade) || quantidade <= 0 || quantidade > 30)
        {
            if (quantidadeInput == "SAIR" || quantidadeInput == "sair")
            {
                Console.WriteLine();
                Console.WriteLine("Programa encerrado pelo usuário.");
                Environment.Exit(0);
            }
            Console.Write("Entrada inválida. Digite um número entre 1 e 30: ");
            quantidadeInput = Console.ReadLine();
        }
        return quantidade;
    }

    static async Task GerarPessoa(int quantidade)
    {
        using (HttpClient client = new HttpClient())
        {

            // URL do endpoint
            string url = "https://www.4devs.com.br/ferramentas_online.php";

            // Dados do corpo da solicitação
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("acao", "gerar_pessoa"),
                new KeyValuePair<string, string>("sexo", "I"), // I para Indiferente
                new KeyValuePair<string, string>("pontuacao", "N"), // N para sem pontuação
                new KeyValuePair<string, string>("idade", "0"), // 0 para idade aleatória
                new KeyValuePair<string, string>("cep_estado", ""), // Campo vazio
                new KeyValuePair<string, string>("txt_qtde", quantidade.ToString()) // Quantidade de pessoas a gerar
            });

            try
            {
                HttpResponseMessage response = await client.PostAsync(url, formData);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var data = JToken.Parse(responseBody);

                List<Pessoa> pessoas = data.ToObject<List<Pessoa>>();

                int nameLength = 0;
                foreach (Pessoa pessoa in pessoas)
                {
                    if (pessoa.nome.Length > nameLength)
                        nameLength = pessoa.nome.Length;
                }
                if (nameLength % 2 == 1)
                { nameLength++; }
                int spacesToAdd = (nameLength - 4) / 2;
                string spaces = new string(' ', spacesToAdd);
                string tituloNome = spaces + "NOME" + spaces;

                Console.WriteLine();
                Console.WriteLine(tituloNome + " |     CPF     | NASCIMENTO | ESTADO |  TELEFONE  | " +
                    " PLACA  | ANO  |      MARCA      | MODELO");
                Console.WriteLine();

                foreach (Pessoa pessoa in pessoas)
                {
                    Console.Write(pessoa.nome.PadRight(nameLength) + " | ");
                    Console.Write(pessoa.cpf + " | ");
                    Console.Write(pessoa.data_nasc + " | ");
                    Console.Write("  " + pessoa.estado + "   | ");
                    Console.Write(pessoa.telefone_fixo + " | ");

                    Veiculo veiculo = await GerarVeiculo(pessoa.estado);

                    Console.Write(veiculo.Placa + " | ");
                    Console.Write(veiculo.Ano + " | ");
                    Console.Write(veiculo.Marca.PadRight(15) + " | ");
                    Console.WriteLine(veiculo.Modelo);                   
                }
                Console.WriteLine();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Erro na requisição: {e.Message}");
            }
        }
    }

    static async Task<Veiculo> GerarVeiculo(string estado)
    {
        string url = "https://www.4devs.com.br/ferramentas_online.php";

        Veiculo veiculo = new Veiculo();

        var parameters = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("acao", "gerar_veiculo"),
            new KeyValuePair<string, string>("estado", estado),
            new KeyValuePair<string, string>("idade", "0"),
            new KeyValuePair<string, string>("categoria", "0"),
            new KeyValuePair<string, string>("cambio", "0"),
            new KeyValuePair<string, string>("carroceria", "0"),
            new KeyValuePair<string, string>("gasolina", "1")
        });

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.PostAsync(url, parameters);

                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync();

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(responseData);

                var placaNode = htmlDocument.DocumentNode.SelectSingleNode("//*[@id='placa_veiculo']");
                var marcaNode = htmlDocument.DocumentNode.SelectSingleNode("//*[@id='marca']");
                var modeloNode = htmlDocument.DocumentNode.SelectSingleNode("//*[@id='modelo']");
                var anoNode = htmlDocument.DocumentNode.SelectSingleNode("//*[@id='ano']");

                if (marcaNode != null)
                {
                    veiculo.Placa = placaNode.GetAttributeValue("value", "");
                    veiculo.Marca = marcaNode.GetAttributeValue("value", "");
                    veiculo.Modelo = modeloNode.GetAttributeValue("value", "");
                    veiculo.Ano = anoNode.GetAttributeValue("value", "");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }

        return veiculo;
    }

}