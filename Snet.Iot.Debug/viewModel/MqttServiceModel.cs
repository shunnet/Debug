using Snet.Iot.Debug.template;
using Snet.Mqtt.service;
using Snet.Utility;
using static Snet.Mqtt.service.MqttServiceData;
namespace Snet.Iot.Debug.viewModel
{
    /// <summary>MQTT TCP 服务调试模型。</summary>
    public sealed class MqttServiceModel : MqServiceTemplateModel<Basics>
    {
        public MqttServiceModel()
        {
            //初始化基础数据
            BasicsData = new Basics();
            //设置对象
            MqService = MqttServiceOperate.Instance(BasicsData);
            //工具标题
            Key = "MqttService";
        }

        public override async Task OnAsync()
        {
            await InitializeAsync();
            MqttServiceOperate factory = MqService as MqttServiceOperate ?? throw new InvalidOperationException("MQTT service has not been initialized.");
            var creation = await factory.CreateInstanceAsync(BasicsData.ToJson(true));
            MqttServiceOperate mq = creation.ResultData as MqttServiceOperate ?? throw new InvalidOperationException(creation.Message ?? "MQTT service creation failed.");
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
            MqttServiceOperate mq = MqService as MqttServiceOperate ?? throw new InvalidOperationException("MQTT service has not been initialized.");
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
