using Opc.Ua;
using Snet.Iot.Debug.model;
using Snet.Iot.Debug.viewModel;
using Snet.Opc.core;

namespace Snet.Iot.Debug.Tests;

/// <summary>OPC UA 节点导出与订阅遍历的回归测试。</summary>
[TestClass]
public sealed class OpcUaNodeExportTests
{
    /// <summary>递归订阅只返回每个叶节点一次。</summary>
    [TestMethod]
    public void SubscribeNodes_ReturnsEachLeafOnce()
    {
        using var root = new OpcUaNodeBrowseStructuralBody();
        root.Children.Add(CreateLeaf("ns=2;s=A"));
        var branch = new OpcUaNodeBrowseStructuralBody();
        branch.Children.Add(CreateLeaf("ns=2;s=B"));
        branch.Children.Add(CreateLeaf("ns=2;s=C"));
        root.Children.Add(branch);

        List<string> addresses = OpcUaNodeBrowsingModel.SubscribeNodes(root);

        CollectionAssert.AreEqual(new[] { "ns=2;s=A", "ns=2;s=B", "ns=2;s=C" }, addresses);
    }

    /// <summary>空子节点集合仍代表叶节点，导出时不能被遗漏。</summary>
    [TestMethod]
    public void CollectLeafNodes_IncludesNodeWithEmptyChildren()
    {
        var leaf = new NodeBody { Nodes = [] };
        var destination = new List<NodeBody>();

        OpcUaNodeBrowsingModel.CollectLeafNodes(leaf, destination);

        Assert.HasCount(1, destination);
        Assert.AreSame(leaf, destination[0]);
    }

    /// <summary>导出文件名必须替换 Windows 非法字符并为纯空白提供回退值。</summary>
    [TestMethod]
    public void SanitizeFileName_ReplacesInvalidCharactersAndProvidesFallback()
    {
        Assert.AreEqual("A_B", OpcUaNodeBrowsingModel.SanitizeFileName("A:B"));
        Assert.AreEqual("Node", OpcUaNodeBrowsingModel.SanitizeFileName("   "));
    }

    /// <summary>创建带 OPC UA NodeId 的叶节点。</summary>
    private static OpcUaNodeBrowseStructuralBody CreateLeaf(string nodeId)
    {
        return new OpcUaNodeBrowseStructuralBody
        {
            NodeID = new ReferenceDescription { NodeId = ExpandedNodeId.Parse(nodeId) }
        };
    }
}
