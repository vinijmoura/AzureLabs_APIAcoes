using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using APIAcoes.Models;

namespace APIAcoes.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AcoesController : ControllerBase
    {
        private readonly ILogger<AcoesController> _logger;

        public AcoesController(ILogger<AcoesController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Resultado), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]        
        public async Task<Resultado> Post(
            [FromServices] IConfiguration configuration,
            Acao acao)
        {
            var cotacaoAcao = new CotacaoAcao()
            {
                Codigo = acao.Codigo,
                Valor = acao.Valor,
                CodCorretora = configuration["Corretora:Codigo"],
                NomeCorretora = configuration["Corretora:Nome"]
            };
            var conteudoAcao = JsonSerializer.Serialize(cotacaoAcao);
            _logger.LogInformation($"Dados: {conteudoAcao}");

            var body = Encoding.UTF8.GetBytes(conteudoAcao);

            string queue = configuration["Queue-AzureServiceBus"];
            var client = new QueueClient(
                configuration.GetConnectionString("AzureServiceBus"),
                queue);
            await client.SendAsync(new Message(body));
            _logger.LogInformation(
                $"Azure Service Bus - Envio para a queue {queue} concluído");

            return new Resultado()
            {
                Mensagem = "Informações de ação enviadas com sucesso!"
            };
        }
     }
}