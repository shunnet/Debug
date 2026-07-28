using Snet.Iot.Debug.template;
using Snet.Mqtt.service.websocket;
using Snet.Utility;
using static Snet.Mqtt.service.websocket.MqttWebSocketServiceData;
namespace Snet.Iot.Debug.viewModel
{
    public class MqttWebSocketServiceModel : MqServiceTemplateModel<Basics>
    {
        public MqttWebSocketServiceModel()
        {
            //初始化基础数据
            BasicsData = new Basics();
            //设置对象
            MqService = MqttWebSocketServiceOperate.Instance(BasicsData);
            //工具标题
            Key = "MqttWsService";
            LanguageHandler_OnLanguageEventAsync(null, null);
        }

        public override async Task OnAsync()
        {
            MqttWebSocketServiceOperate Mq = MqService.GetSource<MqttWebSocketServiceOperate>();
            var mq = (await Mq.CreateInstanceAsync(BasicsData.ToJson(true))).ResultData.GetSource<MqttWebSocketServiceOperate>();
            var result = await mq.OnAsync();
            await uiMessage_InfoEvent.ShowAsync(result.Message);
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
            MqttWebSocketServiceOperate mq = MqService.GetSource<MqttWebSocketServiceOperate>();
            var result = await mq.OffAsync();
            await uiMessage_InfoEvent.ShowAsync(result.Message);
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
