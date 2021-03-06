﻿using System.IO;

namespace ExtjsWd.js
{
    public static class JSCommands
    {
        public static void CloseAllMessageBoxes()
        {
            const string closeMsgBoxesViaExtjs = @"Ext.Msg.close();";
            ScenarioFixture.Instance.EvalJS(closeMsgBoxesViaExtjs);
        }

        public static void CloseAllTooltips()
        {
            EvalJS("[].forEach.call(document.querySelectorAll('.x-tip'), function(el) { el.remove(); })");
        }

        public static void CloseAllWindows()
        {
            const string killWindowsViaExtjs = @"
                for(var i in Ext.WindowMgr.list) {
                    var val = Ext.WindowMgr.list[i];
                    if(val.isComponent && val.isVisible()) {
                        val.destroy();
                    }
                }";
            EvalJS(killWindowsViaExtjs);
        }

        public static void CloseAnyOpenGarbage()
        {
            CloseAllMessageBoxes();
            CloseAllWindows();
            CloseAllTooltips();
        }

        public static void EvalJSFile(string path)
        {
            EvalJS(ReadJSFile(path));
        }

        private static void EvalJS(string js)
        {
            ScenarioFixture.Instance.EvalJS(js);
        }

        private static string ReadJSFile(string relativePath)
        {
            var assembly = typeof(JSCommands).Assembly;
            return new StreamReader(
                    assembly.GetManifestResourceStream(assembly.GetName().Name + "." + relativePath.Replace("/", ".")))
                    .ReadToEnd();
        }
    }
}