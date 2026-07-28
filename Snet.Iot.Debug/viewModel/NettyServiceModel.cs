using Snet.Iot.Debug.template;
using Snet.Netty.service;
using Snet.Utility;
using static Snet.Netty.service.NettyServiceData;
namespace Snet.Iot.Debug.viewModel
{
    public class NettyServiceModel : MqServiceTemplateModel<Basics>
    {
        public NettyServiceModel()
        {
            //初始化基础数据
            BasicsData = new Basics();
            //设置对象
            MqService = NettyServiceOperate.Instance(BasicsData);
            //工具标题
            Key = "NettyService";

            LanguageHandler_OnLanguageEventAsync(null, null);
        }

        public override async Task OnAsync()
        {
            NettyServiceOperate Mq = MqService.GetSource<NettyServiceOperate>();
            var mq = (await Mq.CreateInstanceAsync(BasicsData.ToJson(true))).ResultData.GetSource<NettyServiceOperate>();
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
            NettyServiceOperate mq = MqService.GetSource<NettyServiceOperate>();
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
