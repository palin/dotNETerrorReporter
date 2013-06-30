using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Reflection;

/* Autor:
 * Maciej Paliński
 * Informatyka
 * Wydział Elektryczny
 * Semestr 7 
 */ 
namespace ErrorReporter
{
  public class ErrorReporter
  {
    private string url;
    private string method = "POST";
    private string errorMessage = "Czy chcesz wysłać informacje o błędzie do administratora (wymaga połączenia z Internetem)?";
    private string errorCaption = "Wystąpił błąd!";
    private string requestFailed = "Nie udało się wysłać zgłoszenia błędu. Jeśli błąd pojawi się ponownie, spróbuj wysłać zgłoszenie jeszcze raz. W przypadku problemów zgłoś się do autora programu";
    private string requestSucceed = "Pomyślnie wysłano zgłoszenie błędu.";
    private bool autoSend = true;
    private bool autoConfirmation = false;
    XmlDocument doc = null;
    XmlElement root = null;

    public ErrorReporter() { }

    public ErrorReporter(string _url)
    {
      url = _url;
    }

    public ErrorReporter(string _url, string _method)
    {
      url = _url;
      method = _method;
    }

    public ErrorReporter(string _url, string _method, bool _autosend)
    {
      url = _url;
      method = _method;
      autoSend = _autosend;
    }

    public ErrorReporter(string _url, string _method, bool _autosend, bool _autoconfirmation)
    {
      url = _url;
      method = _method;
      autoSend = _autosend;
      autoConfirmation = _autoconfirmation;
    }

    /// <summary>
    /// Gets HTTP request method and returns string or sets http request method: "POST" (default), "PUT", "GET".
    /// </summary>
    public string RequestMethod
    {
      get { return method; }
      set { if (value.ToString().ToUpper() == "POST" || value.ToUpper() == "PUT" || value.ToUpper() == "GET") method = value.ToString().ToUpper(); }
    }

    /// <summary>
    /// Gets or sets HTTP request URI.
    /// </summary>
    public string RequestUri
    {
      get { return url; }
      set { url = value.ToString(); }
    }

    /// <summary>
    /// Gets or sets text, which appears in content of MessageBox after exception.
    /// </summary>
    public string ErrorMessage
    {
      get { return errorMessage; }
      set { errorMessage = value.ToString(); }
    }

    /// <summary>
    /// Gets or sets text, which appears in title of MessageBox after exception.
    /// </summary>
    public string ErrorCaption
    {
      get { return errorCaption; }
      set { errorCaption = value.ToString(); }
    }

    /// <summary>
    /// Gets or sets text, which appears in content of MessageBox after failed HTTP request.
    /// </summary>
    public string RequestFailedMessage
    {
      get { return requestFailed; }
      set { requestFailed = value.ToString(); }
    }

    /// <summary>
    /// Gets or sets text, which appears in content of MessageBox after succeeded (200, OK) HTTP request.
    /// </summary>
    public string RequestSucceedMessage
    {
      get { return requestSucceed; }
      set { requestSucceed = value.ToString(); }
    }

    /// <summary>
    /// Decides if request after exception should be sent without user's confirmation. Possible values: true (no confirmation- default), false (display MessageBox with Yes/No)
    /// </summary>
    public bool AutoSend
    {
      get { return autoSend; }
      set { if (autoSend == true || autoSend == false) autoSend = value; }
    }

    /// <summary>
    /// Decides if MessageBox with confirmation after HTTP request is shown. Possible values: true (do not display MessageBox), false (default- display MessageBox with OK)
    /// </summary>
    public bool AutoConfirmation
    {
      get { return autoConfirmation; }
      set { if (autoConfirmation == true || autoConfirmation == false) autoConfirmation = value; }
    }
    
    /// <summary>
    /// Reports exception information to remote server via HTTP request.
    /// </summary>
    /// <param name="type">Type of the class, from which information about all values (methods, fields etc.) should be retrieved.</param>
    /// <param name="obj">Object of class, which was defined as type. Most of cases you can use: this.</param>
    /// <param name="metaData">User-defined data.</param> 
    /// <param name="stackframe">New object of System.Diagnostics.StackFrame.</param>
    /// <param name="ex">Object of Exception class.</param>
    public void Report(Type type, Object obj, string metaData = null, StackFrame stackframe = null, Exception ex = null)
    {
      bool success = false;

      if (autoSend) success = buildXmlAndSendRequest(type, obj, metaData, stackframe, ex);
      else
        if ((DialogResult)MessageBox.Show(errorMessage, errorCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Stop) == DialogResult.Yes) success = buildXmlAndSendRequest(type, obj, metaData, stackframe, ex);

      if (!autoConfirmation)
      {
        if (success) MessageBox.Show(requestSucceed);
        else MessageBox.Show(requestFailed);
      }
    }

    /// <summary>
    /// Builds XML Document with given data. Returns string.
    /// </summary>
    /// <param name="doc">XmlDocument</param>
    /// <param name="type">Type</param>
    /// <param name="ex">Exception</param>
    /// <param name="obj">Object</param>
    /// <returns>String</returns>
    private string buildXmlDocument(XmlDocument doc, Type type, Exception ex, Object obj, StackFrame stackframe, string metaData)
    {
      createXmlDeclaration();
      root = createRootTag();
      createExceptionDump(ex);
      createLocalVariablesDump(stackframe);
      createClassFieldsDump(type, obj);
      createUserMetaDataTag(metaData);

      return doc.InnerXml.ToString();
    }

    private void createUserMetaDataTag(string metaData)
    {
      XmlElement exTag = doc.CreateElement("meta-data");

      if (metaData != null && metaData.Length > 0)
      {
        exTag.InnerText = metaData.ToString();
      }

      root.AppendChild(exTag);
    }

    /// <summary>
    /// Creates first tag of xml document- xml declaration with given xml version and encodig
    /// </summary>
    private void createXmlDeclaration()
    {
      XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "utf-8", "");
      doc.AppendChild(declaration);
    }

    /// <summary>
    /// Creates second tag- root tag
    /// </summary>
    /// <returns>XmlElement</returns>
    private XmlElement createRootTag()
    {
      XmlElement root = doc.CreateElement("", "xml_content", "");

      root.SetAttribute("date-time", DateTime.Now.ToString());
      root.SetAttribute("app-version", Application.ProductVersion.ToString());
      doc.AppendChild(root);

      return root;
    }

    private void createExceptionDump(Exception ex)
    {
      XmlElement exTag = doc.CreateElement("exception-info");

      if (ex != null)
      {
        exTag.SetAttribute("message", ex.Message);
        exTag.SetAttribute("source", ex.Source);
        exTag.SetAttribute("stack-trace", ex.StackTrace.ToString());
        try
        {
          exTag.SetAttribute("inner-exception", ex.InnerException.ToString());
        }
        catch { }
        exTag.SetAttribute("target-site", ex.TargetSite.ToString());
      }
      else
      {
        exTag.InnerText = "Exception object not found";
      }

      root.AppendChild(exTag);
    }

    private void createLocalVariablesDump(StackFrame stackFrame)
    {
      XmlElement exTag = doc.CreateElement("local-variables");

      if (stackFrame != null)
      {
        MethodBase methodBase = stackFrame.GetMethod();
        ParameterInfo[] pms = methodBase.GetParameters();
        MethodBody body = methodBase.GetMethodBody();

        exTag.SetAttribute("init-locals", body.InitLocals.ToString());
        exTag.SetAttribute("local-variables-count", body.LocalVariables.Count.ToString());

        IList<LocalVariableInfo> lv = methodBase.GetMethodBody().LocalVariables; //List of Local variables declared in the body

        foreach (LocalVariableInfo lvi in lv)
        {
          XmlElement el = doc.CreateElement("var");

          el.SetAttribute("is-pinned", lvi.IsPinned.ToString());
          el.SetAttribute("local-index", lvi.LocalIndex.ToString());
          el.SetAttribute("local-type-fullname", lvi.LocalType.FullName.ToString());

          exTag.AppendChild(el);
        }
      }
      else
      {
        exTag.InnerText = "StackFrame object not found";
      }

      root.AppendChild(exTag);
    }

    private void createClassFieldsDump(Type type, Object obj)
    {
      XmlElement exTag = doc.CreateElement("class-fields");
      exTag.SetAttribute("name", type.Name);
      exTag.SetAttribute("full-name", type.FullName);
      exTag.SetAttribute("namespace", type.Namespace);

      foreach (FieldInfo f in type.GetFields())
      {
        XmlElement el = doc.CreateElement("field");

        el.SetAttribute("name", f.Name);
        el.SetAttribute("field-type", f.FieldType.Name);

        try
        {
          el.SetAttribute("value", f.GetValue(obj).ToString());
        }
        catch { el.SetAttribute("value", "error"); }

        exTag.AppendChild(el);
      }

      root.AppendChild(exTag);
    }

    private bool sendHttpRequest(string xmlDocument)
    {
      WebRequest webrequest = WebRequest.Create(RequestUri);

      webrequest.ContentType = "application/xml; encoding='utf-8'";
      webrequest.Method = RequestMethod;
      byte[] bytes = Encoding.ASCII.GetBytes(xmlDocument.ToString());
      webrequest.ContentLength = bytes.Length;
      Stream os = null;
      WebResponse response = null;

      try
      {
        os = webrequest.GetRequestStream();
        os.Write(bytes, 0, bytes.Length);
        os.Close();

        response = webrequest.GetResponse();
      }
      catch { return false; }

      return true;
    }

    private bool buildXmlAndSendRequest(Type type, Object obj, string metaData, StackFrame stackframe, Exception ex)
    {
      doc = new XmlDocument();
      return sendHttpRequest(buildXmlDocument(doc, type, ex, obj, stackframe, metaData));
    }
  }
}
