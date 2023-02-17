using KafkaReaderServer.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KafkaReaderServer.Controllers;

[Route("api/kafka")]
[ApiController]
public class KafkaController : ControllerBase
{
    private readonly IKafkaUnitOfWork _kafkaUnitOfWork;

    public KafkaController(IKafkaUnitOfWork kafkaUnitOfWork)
    {
        _kafkaUnitOfWork = kafkaUnitOfWork;
    }

    [HttpGet("topics")]
    public ActionResult<IEnumerable<string>> GetKafkaTopics()
    {
        var topics = _kafkaUnitOfWork.GetKafkaTopics();
        return Ok(topics);
    }

    [HttpPost("messages")]
    public IActionResult GetKafkaTopicMessages([FromBody] string topic, CancellationToken cancellationToken)
    {
       _kafkaUnitOfWork.ConsumeKafkaTopic(topic, cancellationToken);
       return Accepted();
    }
}