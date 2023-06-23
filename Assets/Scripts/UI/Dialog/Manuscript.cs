using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Manuscript
{
    public class Dialog
    {
        public class DialogNode //Each node on the dialog tree is a DialogNode that contains a set amount of lines and usually ends in some kind of prompt
        {
            public class Line
            {
                public class CharacterIdentity
                {
                    public string name;
                    public Sprite mugshot;

                    public CharacterIdentity(string name_in)
                    {
                        name = name_in;
                        mugshot = null;
                    }
                }

                public Line(string line, CharacterIdentity identity)
                {
                    myLine = line;
                    myIdentity = identity;
                }
                public string myLine;
                public CharacterIdentity myIdentity;
            }
            public class PromptOption
            {
                public string promptName;
                public DialogNode destinationDialog;
                public Action action;

                public PromptOption(string promptName_in, DialogNode destinationDialog_in)
                {
                    promptName = promptName_in;
                    destinationDialog = destinationDialog_in;
                }
                public PromptOption(string promptName_in, Action action)
                {
                    promptName = promptName_in;
                    destinationDialog = null;
                    this.action = action;
                }
            }
            public List<Line> lines = new List<Line>() { };
            public List<PromptOption> options = new List<PromptOption>() { };
            
            public DialogNode()
            {

            }
            public DialogNode(string line, string identity)
            {
                lines.Add(new Line(line, new Line.CharacterIdentity(identity)));
            }
            public DialogNode(List<Line> lines_in, List<PromptOption> options_in)
            {
                lines = lines_in;
                options = options_in;
            }
        }
        DialogNode startNode;
        public DialogNode currentNode;
        public Dialog()
        {
            startNode = new DialogNode();
            currentNode = startNode;
        }
        public Dialog(DialogNode startNode_in)
        {
            startNode = startNode_in;
            currentNode = startNode_in;
        }
        public void Add(string text, string identity)
        {
            startNode.lines.Add(new DialogNode.Line(text, new DialogNode.Line.CharacterIdentity(identity)));
        }
        public void Add(DialogNode.PromptOption promptOption)
        {
            startNode.options.Add(promptOption);
        }
    }
}