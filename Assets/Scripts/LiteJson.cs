using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
// 小型 JSON 读写：存档格式与网页版轻松版导出的 .json 完全一致（可以互相导入）。
namespace HarbourYear.Lite
{
    public class JObj:Dictionary<string,object>
    {
        public readonly List<string> Order=new List<string>();
        public new object this[string k]{get{object v;return TryGetValue(k,out v)?v:null;}set{if(!ContainsKey(k))Order.Add(k);base[k]=value;}}
        public bool Has(string k){return ContainsKey(k);}
        public string S(string k){return this[k] as string;}
        public double D(string k){var v=this[k];if(v is double)return (double)v;throw new FormatException(k+" 不是数字");}
        public int I(string k){double d=D(k);if(d!=Math.Floor(d)||Math.Abs(d)>int.MaxValue)throw new FormatException(k+" 不是整数");return (int)d;}
        public int? NI(string k){return this[k]==null?(int?)null:I(k);}
        public double? ND(string k){return this[k]==null?(double?)null:D(k);}
        public bool B(string k){var v=this[k];if(v is bool)return (bool)v;throw new FormatException(k+" 不是布尔值");}
        public JObj O(string k){var v=this[k] as JObj;if(v==null)throw new FormatException(k+" 缺失");return v;}
        public List<object> A(string k){var v=this[k] as List<object>;if(v==null)throw new FormatException(k+" 缺失");return v;}
    }
    public static class Json
    {
        public static string Num(double d)
        {
            if(double.IsNaN(d)||double.IsInfinity(d))return "null";if(d==0)return "0";
            if(d==Math.Floor(d)&&Math.Abs(d)<1e15)return ((long)d).ToString(CultureInfo.InvariantCulture);
            return d.ToString("R",CultureInfo.InvariantCulture);
        }
        public static string Write(object v,bool pretty=false){var b=new StringBuilder();W(b,v,pretty?0:-1);return b.ToString();}
        static void Nl(StringBuilder b,int depth){if(depth<0)return;b.Append('\n').Append(' ',depth*2);}
        static void W(StringBuilder b,object v,int depth)
        {
            if(v==null)b.Append("null");else if(v is string)Str(b,(string)v);else if(v is bool)b.Append((bool)v?"true":"false");
            else if(v is double)b.Append(Num((double)v));else if(v is int)b.Append(((int)v).ToString(CultureInfo.InvariantCulture));else if(v is long)b.Append(((long)v).ToString(CultureInfo.InvariantCulture));
            else if(v is JObj)
            {
                var o=(JObj)v;if(o.Order.Count==0){b.Append("{}");return;}b.Append('{');bool first=true;
                foreach(var k in o.Order){if(!first)b.Append(',');first=false;Nl(b,depth<0?-1:depth+1);Str(b,k);b.Append(depth<0?":":": ");W(b,o[k],depth<0?-1:depth+1);}
                Nl(b,depth);b.Append('}');
            }
            else if(v is System.Collections.IEnumerable)
            {
                var items=((System.Collections.IEnumerable)v).Cast<object>().ToList();if(items.Count==0){b.Append("[]");return;}b.Append('[');
                for(int i=0;i<items.Count;i++){if(i>0)b.Append(',');Nl(b,depth<0?-1:depth+1);W(b,items[i],depth<0?-1:depth+1);}
                Nl(b,depth);b.Append(']');
            }
            else throw new ArgumentException("无法写入 "+v.GetType());
        }
        static void Str(StringBuilder b,string s)
        {
            b.Append('"');
            foreach(char c in s)
            {
                switch(c){case '"':b.Append("\\\"");break;case '\\':b.Append("\\\\");break;case '\n':b.Append("\\n");break;case '\r':b.Append("\\r");break;case '\t':b.Append("\\t");break;case '\b':b.Append("\\b");break;case '\f':b.Append("\\f");break;
                default:if(c<0x20)b.Append("\\u").Append(((int)c).ToString("x4"));else b.Append(c);break;}
            }
            b.Append('"');
        }
        public static object Parse(string s){int i=0;var v=Value(s,ref i);Ws(s,ref i);if(i!=s.Length)throw new FormatException("JSON 结尾有多余内容");return v;}
        static void Ws(string s,ref int i){while(i<s.Length&&(s[i]==' '||s[i]=='\n'||s[i]=='\r'||s[i]=='\t'||s[i]=='﻿'))i++;}
        static object Value(string s,ref int i)
        {
            Ws(s,ref i);if(i>=s.Length)throw new FormatException("JSON 不完整");char c=s[i];
            if(c=='{')
            {
                var o=new JObj();i++;Ws(s,ref i);if(s[i]=='}'){i++;return o;}
                while(true){Ws(s,ref i);if(s[i]!='"')throw new FormatException("缺少键名");var k=Strng(s,ref i);Ws(s,ref i);if(s[i++]!=':')throw new FormatException("缺少冒号");o[k]=Value(s,ref i);Ws(s,ref i);if(s[i]==','){i++;continue;}if(s[i]=='}'){i++;return o;}throw new FormatException("对象格式错误");}
            }
            if(c=='[')
            {
                var a=new List<object>();i++;Ws(s,ref i);if(s[i]==']'){i++;return a;}
                while(true){a.Add(Value(s,ref i));Ws(s,ref i);if(s[i]==','){i++;continue;}if(s[i]==']'){i++;return a;}throw new FormatException("数组格式错误");}
            }
            if(c=='"')return Strng(s,ref i);
            if(s.Length-i>=4&&string.CompareOrdinal(s,i,"true",0,4)==0){i+=4;return true;}
            if(s.Length-i>=5&&string.CompareOrdinal(s,i,"false",0,5)==0){i+=5;return false;}
            if(s.Length-i>=4&&string.CompareOrdinal(s,i,"null",0,4)==0){i+=4;return null;}
            int start=i;while(i<s.Length&&"+-0123456789.eE".IndexOf(s[i])>=0)i++;
            double d;if(start==i||!double.TryParse(s.Substring(start,i-start),NumberStyles.Float,CultureInfo.InvariantCulture,out d))throw new FormatException("无法识别的 JSON 值");return d;
        }
        static string Strng(string s,ref int i)
        {
            var b=new StringBuilder();i++;
            while(true)
            {
                if(i>=s.Length)throw new FormatException("字符串未结束");char c=s[i++];if(c=='"')return b.ToString();
                if(c!='\\'){b.Append(c);continue;}char e=s[i++];
                switch(e){case 'n':b.Append('\n');break;case 'r':b.Append('\r');break;case 't':b.Append('\t');break;case 'b':b.Append('\b');break;case 'f':b.Append('\f');break;case 'u':b.Append((char)Convert.ToInt32(s.Substring(i,4),16));i+=4;break;default:b.Append(e);break;}
            }
        }
    }
}
