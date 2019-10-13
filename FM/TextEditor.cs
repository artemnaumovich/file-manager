﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace FM
{

    
    public partial class TextEditor : Form
    {

        public EditorController editorController = new EditorController();
        public string activePath;
        FileInfo activeFile;
        public TextEditor(FileInfo fi)
        {
            InitializeComponent();
            MyInit();
            LoadText(fi);
        }

        private void MyInit()
        {

            richTextBox.WordWrap = false;
            richTextBox.ScrollBars = RichTextBoxScrollBars.ForcedBoth;
            
        }

        private void LoadText(FileInfo fi)
        {
            StreamReader sr = new StreamReader(fi.FullName, Encoding.UTF8);
            activePath = fi.FullName;
            richTextBox.Text = sr.ReadToEnd();
            activeFile = fi;
            sr.Close();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorController.saveFile(richTextBox, activePath);
        }


        private void TextEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            var dlgRes = MessageBox.Show("Do you want to save file before closing?", "", MessageBoxButtons.YesNo);
            if (dlgRes == DialogResult.Yes)
                editorController.saveFile(richTextBox, activePath);
        }

        private void DeleteEmptyTagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorController.deleteEmptyTags(richTextBox);
        }


        private void FindUniqueWordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorController.findUniqueWords(richTextBox, activePath);
        }

        private void FindToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorController.find(richTextBox);

        }

        public class EditorController
        {
            public void saveFile(RichTextBox richTextBox, string activePath)
            {
                
                richTextBox.Text += Environment.NewLine;
                File.WriteAllText(activePath, richTextBox.Text.Replace("\n", Environment.NewLine));

            }

          

            public void deleteEmptyTags(RichTextBox richTextBox)
            {
                try
                {
                    var text = richTextBox.Text;
                    var document = XDocument.Parse(text);
                    document.Descendants()
                            .Where(e =>     e.IsEmpty || String.IsNullOrWhiteSpace(e.Value))
                            .Remove();
                    richTextBox.Text = document.ToString();
                }
                catch (Exception e)
                {
                    ErrorMessage errorMessage = new ErrorMessage(e.Message);
                    errorMessage.ShowDialog();
                }
            }

            private List<string> filter(string[] words)
            {
                List<string> filterWords = new List<string>();
                foreach (string word in words)
                {
                    bool letters = word.Any(char.IsLetter);
                    if (letters)
                    {
                        
                        //Regex rgx = new Regex("[^a-zA-Zа-яА-Я ]");
                        //string wordUpdate = rgx.Replace(word, "");
                        filterWords.Add(word.ToLower());
                    }
                }

                Dictionary<string, int> dict = new Dictionary<string, int>();
                foreach (string word in filterWords)
                {
                    Console.WriteLine("!" + word + "!");
                    if (dict.ContainsKey(word))
                    {
                        dict[word]++;
                    }
                    else
                    {
                        dict.Add(word, 1);
                    }
                }

                List<string> uniqueWords = new List<string>();
                foreach (string word in dict.Keys)
                {
                    if (dict[word] == 1)
                    {
                        uniqueWords.Add(word);
                    }
                }

                return uniqueWords;

                

            } 

            public void findUniqueWords(RichTextBox richTextBox, string activePath)
            {
                string text = richTextBox.Text;
                text.Replace("\n", " ");
                List<string> words = filter(text.Split(new char[] { ' ', ',', '.', '?', '!', '\n'}, StringSplitOptions.RemoveEmptyEntries));
                ShowUniqueWords showUniqueWords = new ShowUniqueWords(words);
                showUniqueWords.ShowDialog();
            }

            public void find(RichTextBox richTextBox)
            {
                FindByPattern findByPattern = new FindByPattern(richTextBox.Text);
                findByPattern.ShowDialog();
            }
        }

        
    }
}
