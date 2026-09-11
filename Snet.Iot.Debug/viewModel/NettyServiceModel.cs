using Snet.Iot.Debug.template;
using Snet.Netty.service;
using Snet.Utility;
using static Snet.Netty.service.NettyServiceData;
namespace Snet.Iot.Debug.viewModel
{
    /// <summary>Netty 服务调试模型。</summary>
    public sealed class NettyServiceModel : MqServiceTemplateModel<Basics>
    {
        public NettyServiceModel()
        {
            //初始化基础数据
            BasicsData = new Basics();
            //设置对象
            MqService = NettyServiceOperate.Instance(BasicsData);
            //工具标题
            Key = "NettyService";

        }

        public override async Task OnAsync()
        {
            await InitializeAsync();
            NettyServiceOperate factory = MqService as NettyServiceOperate ?? throw new InvalidOperationException("Netty service has not been initialized.");
            var creation = await factory.CreateInstanceAsync(BasicsData.ToJson(true));
            NettyServiceOperate mq = creation.ResultData as NettyServiceOperate ?? throw new InvalidOperationException(creation.Message ?? "Netty service creation failed.");
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
            NettyServiceOperate mq = MqService as NettyServiceOperate ?? throw new InvalidOperationException("Netty service has not been initialized.");
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
