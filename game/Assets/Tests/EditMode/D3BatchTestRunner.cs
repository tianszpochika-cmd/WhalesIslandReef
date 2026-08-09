using UnityEditor;

namespace Save.Tests
{
    /// <summary>D3 入口别名：复用统一 Save.Tests 批处理门，包含 D2 与 D3 全部 EditMode 测试。</summary>
    public static class D3BatchTestRunner
    {
        public static void RunD3()
        {
            D2BatchTestRunner.RunD2();
        }
    }
}
