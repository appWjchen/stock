using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stock
{
    class CloseApplication : ICloseApplication
    {
        public void closeApplication()
        {
            if (System.Windows.Forms.Application.MessageLoop)
            {
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                System.Environment.Exit(0);
            }
        }
    }
    class AppDoEvents
    {
        public void DoEvents()
        {
            System.Windows.Forms.Application.DoEvents();
        }
    }
    class MessageWriter
    {
        public void showMessage(String message)
        {
            Form1 mainForm = (Form1)System.Windows.Forms.Application.OpenForms[0];
            message = message.Replace("\n", "\r\n");
            mainForm.showTextBoxMessage(message);
        }
        public void appendMessage(String message, bool endPosition)
        {
            Form1 mainForm = (Form1)System.Windows.Forms.Application.OpenForms[0];
            message = message.Replace("\n", "\r\n");
            mainForm.appendTextBoxMessage(message, endPosition);
        }
    }
    class WarningWriter
    {
        public void showMessage(String message)
        {
            Form1 mainForm = (Form1)System.Windows.Forms.Application.OpenForms[0];
            message = message.Replace("\n", "\r\n");
            mainForm.showTextBoxWarning(message);
        }
        public void appendMessage(String message, bool endPosition)
        {
            Form1 mainForm = (Form1)System.Windows.Forms.Application.OpenForms[0];
            message = message.Replace("\n", "\r\n");
            mainForm.appendTextBoxWarning(message, endPosition);
        }
    }
}
