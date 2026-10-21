// 轻松版浏览器存档：导出下载与选择文件导入。
mergeInto(LibraryManager.library, {
  LiteDownload: function (namePtr, textPtr) {
    var name = UTF8ToString(namePtr), text = UTF8ToString(textPtr);
    var url = URL.createObjectURL(new Blob([text], { type: 'application/json' }));
    var a = document.createElement('a'); a.href = url; a.download = name; document.body.appendChild(a); a.click(); a.remove();
    setTimeout(function () { URL.revokeObjectURL(url); }, 1000);
  },
  LitePickFile: function (objPtr, methodPtr) {
    var obj = UTF8ToString(objPtr), method = UTF8ToString(methodPtr);
    var send = function (t) { (typeof SendMessage === 'function' ? SendMessage : Module.SendMessage)(obj, method, t); };
    var input = document.createElement('input'); input.type = 'file'; input.accept = '.json,application/json';
    input.onchange = function () {
      var f = input.files && input.files[0]; if (!f) return;
      if (f.size > 2000000) { send('\u0000too-large'); return; }
      var r = new FileReader(); r.onload = function () { send(String(r.result)); }; r.onerror = function () { send('\u0000unreadable'); }; r.readAsText(f);
    };
    input.click();
  }
});
