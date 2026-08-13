namespace Whalebone.EditorTools
{
    /// <summary>
    /// D16 本地 Unity 批处理测试入口。
    /// 运行时验收使用正式的 D16.PlayMode.Tests，不在此处维护跨播放模式回调。
    /// </summary>
    public static class D16UnityTestEntryPoint
    {
        public static void RunSaveTests()
        {
            Save.Tests.D2BatchTestRunner.RunD2();
        }
    }
}
