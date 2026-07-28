using Snet.Iot.Debug.template;
using Snet.Mqtt.service;
using Snet.Utility;
using static Snet.Mqtt.service.MqttServiceData;
namespace Snet.Iot.Debug.viewModel
{
    public class MqttServiceModel : MqServiceTemplateModel<Basics>
    {
        public MqttServiceModel()
        {
            //初始化基础数据
            BasicsData = new Basics();
            //设置对象
            MqService = MqttServiceOperate.Instance(BasicsData);
            //工具标题
            Key = "MqttService";
            LanguageHandler_OnLanguageEventAsync(null, null);
        }

        public override async Task OnAsync()
        {
            MqttServiceOperate Mq = MqService.GetSource<MqttServiceOperate>();
            var mq = (await Mq.CreateInstanceAsync(BasicsData.ToJson(true))).ResultData.GetSource<MqttServiceOperate>();
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
            MqttServiceOperate mq = MqService.GetSource<MqttServiceOperate>();
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
