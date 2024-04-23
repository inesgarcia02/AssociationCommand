namespace WebApi.Controllers
{
    public interface IRabbitMQHolidayConsumerController
    {
        public void StartConsuming();
        void ConfigQueue(string queueName);
    }
}