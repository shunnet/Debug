using Opc.Ua;
using Snet.Iot.Debug.handler;
using Snet.Iot.Debug.model;

namespace Snet.Iot.Debug.Tests;

/// <summary>分页边界条件的回归测试。</summary>
[TestClass]
public sealed class PagingTests
{
    /// <summary>极大页码不能因整数乘法溢出而回到集合前部。</summary>
    [TestMethod]
    public void ToPagedResult_WithOverflowingOffset_ReturnsEmptyLastPage()
    {
        var source = new List<ReferenceDescription> { new(), new() };

        PagedResult<ReferenceDescription> result = source.ToPagedResult(int.MaxValue, int.MaxValue);

        Assert.IsEmpty(result.Items);
        Assert.IsTrue(result.IsLastPage);
    }

    /// <summary>非法分页参数必须在执行枚举前被拒绝。</summary>
    [TestMethod]
    public void ToPagedResult_WithInvalidArguments_Throws()
    {
        var source = new List<ReferenceDescription>();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.ToPagedResult(-1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => source.ToPagedResult(0, 0));
    }
}
