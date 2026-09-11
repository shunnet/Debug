using Snet.Iot.Debug.template;
using Snet.Mqtt.service.websocket;
using Snet.Utility;
using static Snet.Mqtt.service.websocket.MqttWebSocketServiceData;
namespace Snet.Iot.Debug.viewModel
{
    /// <summary>MQTT WebSocket 服务调试模型。</summary>
    public sealed class MqttWebSocketServiceModel : MqServiceTemplateModel<Basics>
    {
        public MqttWebSocketServiceModel()
        {
            //初始化基础数据
            BasicsData = new Basics();
            //设置对象
            MqService = MqttWebSocketServiceOperate.Instance(BasicsData);
            //工具标题
            Key = "MqttWsService";
        }

        public override async Task OnAsync()
        {
            await InitializeAsync();
            MqttWebSocketServiceOperate factory = MqService as MqttWebSocketServiceOperate ?? throw new InvalidOperationException("MQTT WebSocket service has not been initialized.");
            var creation = await factory.CreateInstanceAsync(BasicsData.ToJson(true));
            MqttWebSocketServiceOperate mq = creation.ResultData as MqttWebSocketServiceOperate ?? throw new InvalidOperationException(creation.Message ?? "MQTT WebSocket service creation failed.");
            var result = await mq.OnAsync();
            await uiMessage_InfoEvent.ShowAsync(result.Message ?? string.Empty);
            if (result.Status)
            {
                mq.OnInfoEventAsync -= Mq_OnInfoEventAsync;
                mq.OnInfoEventAsync += Mq_OnInfoEventAsync;
                mq.OnDataEventAsync -= Mq_OnDataEventAsync;
                mq.OnDataEventAsync += Mq_OnDataEventAsync;
            }
            MqService = mq;
            DeviceStatusFlashing = (await mq.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }

        public override async Task OffAsync()
        {
            await InitializeAsync();
            MqttWebSocketServiceOperate mq = MqService as MqttWebSocketServiceOperate ?? throw new InvalidOperationException("MQTT WebSocket service has not been initialized.");
            var result = await mq.OffAsync();
            await uiMessage_InfoEvent.ShowAsync(result.Message ?? string.Empty);
            if (result.Status)
            {
                mq.OnInfoEventAsync -= Mq_OnInfoEventAsync;
                mq.OnDataEventAsync -= Mq_OnDataEventAsync;
            }
            MqService = mq;
            DeviceStatusFlashing = (await mq.GetStatusAsync()).Status;
            TabSelectedIndex = 1;
        }
    }
}
