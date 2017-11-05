using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Curriculum;
using System;


public class ButtonState {
    public bool[][] grid;
    public bool isEmpty = true;
    public int LectureCount = 0;


    public ButtonState(){
        grid = new bool[5][];
        for (int i = 0; i < grid.Length; i++) {
            grid[i] = new bool[6];
        }
        isEmpty = true;
        LectureCount = 0;
    }

    public ButtonState(bool[][] _grid){
        isEmpty = true;
        LectureCount = 0;
        for (int i = 0; i < grid.Length; i++) {
            for (int j = 0; j < grid[i].Length; j++) {
                grid[i][j] = _grid[i][j];
            }
        }
    }
}

[System.Serializable]
public class CurriculumModule : MonoBehaviour {

    private static int _moduleCount = 1;
    private int _moduleId;

    public KMSelectable submit;
    public KMSelectable[] buttons = new KMSelectable[5];
    public KMAudio Audio;
    public KMBombInfo Info;
    public GameObject[] cells;

    string[][] classes = new string[][] {
        new string[]{"P 1","P 2","P 3","M 1","M 2","M 3"},
        new string[]{"P 1","P 2","P 3","L 1","L 2","L 3"},
        new string[]{"P 1","P 2","P 3","E 1","E 2","E 3"},
        new string[]{"L 1","L 2","L 3","M 1","M 2","M 3"},
        new string[]{"L 1","L 2","L 3","E 1","E 2","E 3"}
    };
    string[] classPairNames = new string[] {
        "(P)hysics - (M)ath", "(P)hilosophy - (L)iterature", "(P)rogramming - (E)conomy", "(L)inguistics - (M)anagement", "(L)ogic - (E)lectronics"};
    string[][] classesOrdered = new string[5][];
    public GameObject[][] heyo = new GameObject[][] { };
    
    //(in that order)
    int[] CorrectSections = new int[5];
    bool solGenerated = false;
    int[] buttonAt = new int[] { 0, 0, 0, 0, 0 };
    bool sleepyGary;
    bool bandPractice;
    string serial;


    ButtonState[][] Sections = new ButtonState[][] {
        new ButtonState[]{new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState() },
        new ButtonState[]{new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState() },
        new ButtonState[]{new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState() },
        new ButtonState[]{new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState() },
        new ButtonState[]{new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState(),new ButtonState() }
    };
    // two dimensions of this: buttons and sections
    // two dimensions of the held bool arrays: grid



    // Use this for initialization
    void Start () {

        _moduleId = _moduleCount++;
        ModuleSetup();
        GetComponent<KMBombModule>().OnActivate += ModuleInit;
    }

    void ModuleSetup() {
        
        //Button interaction init
        submit.OnInteract = delegate () { SubmitPressed(); return false; };
        for (int i = 0; i < 5; i++){
            int j = i;
            buttons[j].OnInteract = delegate () { ButtonPressed(j); return false; };
        }

        //Button random
        List<string[]> temp = new List<string[]>();
        for (int i = 0; i < 5; i++) {
            temp.Add(classes[i]);
        }
        for (int i = 4; i >= 0; i--)
        {
            int ind = UnityEngine.Random.Range(0, i+1);
            classesOrdered[i] = temp.ElementAt(ind);
            temp.RemoveAt(ind);
        }


    }

    private void LightEmUp(int b, int s)
    {
        
        bool[][] temp = Sections[b][(s + 5) % 6].grid;
        for (int i = 0; i < temp.Length; i++)
        {
            for (int j = 0; j < temp[i].Length; j++)
            {
                if (temp[i][j]) {
                    if (cells[i*6+j].GetComponent<MeshRenderer>().material.color == Color.red)
                    {
                        cells[i * 6 + j].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.white);
                       
                    }
                    else cells[i * 6 + j].SetActive(false);
                }
                
            }
        }
        temp = Sections[b][s].grid;
        for (int i = 0; i < temp.Length; i++) {
            for (int j = 0; j < temp[i].Length; j++) {
                if (temp[i][j]) {
                    if (cells[i * 6 + j].activeInHierarchy) {
                        cells[i * 6 + j].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
                    }
                    else cells[i * 6 + j].SetActive(true);
                    
                }
            }
        }
    }

    private void TurnOffBoard()
    {
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 6; j++) {
                cells[i * 6 + j].SetActive(false);
            }
        }
    }

    private void RandomizeLectureCounts()
    {
        int[] LectureCount = new int[10];
        for (int i = 0; i < LectureCount.Length; i++) {
            LectureCount[i] = UnityEngine.Random.Range(2, 4);
        }
        for (int i = 0; i < 5; i++) {
            int pos = Array.IndexOf(classesOrdered, classes[i]);
            int j = i * 2;
            Sections[pos][0].LectureCount = LectureCount[j];
            Sections[pos][1].LectureCount = LectureCount[j];
            Sections[pos][2].LectureCount = LectureCount[j];
            Sections[pos][3].LectureCount = LectureCount[j + 1];
            Sections[pos][4].LectureCount = LectureCount[j + 1];
            Sections[pos][5].LectureCount = LectureCount[j + 1];
        }

    }

    private void AssignCorrectSections(string serial) {
        for (int i = 0; i < 5; i++) {
            char temp = serial.ElementAt(i);
            int pos = Array.IndexOf(classesOrdered, classes[i]);
            if (temp % 2 == 0)
            {
                CorrectSections[pos] = 0;
                classPairNames[i] = classPairNames[i].Insert(classPairNames[i].IndexOf('('), "*");
            }
            else
            {
                CorrectSections[pos] = 3;
                classPairNames[i] = classPairNames[i].Insert(classPairNames[i].LastIndexOf('('), "*");
            }
            CorrectSections[pos] += UnityEngine.Random.Range(0, 3);
        }
    }
    
    private void GenerateOtherCycles()
    {
        for (int i = 0; i < Sections.Length; i++) {
            for (int j = 0; j < Sections[i].Length; j++) {
                if (Sections[i][j].isEmpty) {
                    Stack<Vector2> lecs = new Stack<Vector2>();
                    for (int k = 0; k < 3; k++)
                    {
                        Vector2 temp = new Vector2(UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 6));
                        if (lecs.Contains(temp)) k--;
                        else lecs.Push(temp);
                    }
                    for (int k = 0; k < Sections[i][j].LectureCount; k++) {
                        Vector2 temp = lecs.Pop();
                        Sections[i][j].grid[(int)temp.x][(int)temp.y] = true;
                    }
                    lecs.Clear();
                    Sections[i][j].isEmpty = false;
                }
            }
        }
    }

    private void GenerateSolutions(bool bp, bool sg){
        Stack<Vector2> lecs = new Stack<Vector2>();
        for (int i = 0; i < 15; i++) {
            Vector2 temp = new Vector2(UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 6));
            if (bp && ((int)temp.x == 0 || (int)temp.x == 2) && (int)temp.y > 2) i--;
            else if (sg && (int)temp.y == 0) i--;
            else if (lecs.Contains(temp)) i--;
            else {
                lecs.Push(temp);
            }
        }
        if (!solGenerated)
        {
            string prink = "[Curriculum #{0}] Guaranteed solution: ";
            for (int i = 0; i < 5; i++)
            {
                prink += classesOrdered[i][CorrectSections[i]] + " - ";
                for (int j = 0; j < Sections[i][CorrectSections[i]].LectureCount; j++)
                {
                    Vector2 temp = lecs.Pop();
                    Sections[i][CorrectSections[i]].grid[(int)temp.x][(int)temp.y] = true;
                    Sections[i][CorrectSections[i]].isEmpty = false;
                }
                
            }
            solGenerated = true;
            Debug.LogFormat(prink.TrimEnd(' ', '-'), _moduleId);
        }
        else {
            for (int i = 0; i < 5; i++)
            {
                int randSec = UnityEngine.Random.Range(0, 3);
                if (CorrectSections[i] < 3) randSec += 3;
                while (!Sections[i][randSec].isEmpty) {
                    randSec = UnityEngine.Random.Range(0, 3);
                    if (CorrectSections[i] < 3) randSec += 3;
                }
                for (int j = 0; j < Sections[i][randSec].LectureCount; j++) {
                    Vector2 temp = lecs.Pop();
                    Sections[i][randSec].grid[(int)temp.x][(int)temp.y] = true;
                    Sections[i][randSec].isEmpty = false;
                }

            }
        }
        lecs.Clear();
    }

    void ModuleInit() {

        serial = Info.GetSerialNumber();
        bandPractice = Info.GetBatteryCount() > 2;
        sleepyGary = Info.GetIndicators().Count() == 0;

        TurnOffBoard();

        RandomizeLectureCounts();

        AssignCorrectSections(serial);

        Debug.LogFormat("[Curriculum #{0}] Classes on the buttons, in reading order (Asterisks indicate the correct class in each pair):", _moduleId);
        for (int i = 0; i < 5; i++) {
            Debug.LogFormat("[Curriculum #{0}] {1}", _moduleId, classPairNames[Array.IndexOf(classes, classesOrdered[i])]);
        }
        
        
        //Teh main solution
        GenerateSolutions(bandPractice, sleepyGary);
        //Generate multiple false solutions to avoid button mashing
        int FalseSolves = UnityEngine.Random.Range(1, 3);
        for (int i = 0; i < FalseSolves; i++)
        {
            GenerateSolutions(false, false);
        }

        for (int i = 0; i < 5 - FalseSolves; i++)
        {
            GenerateOtherCycles();
        }

        //Button text & light init
        for (int i = 0; i < 5; i++)
        {
            buttons[i].GetComponentInChildren<TextMesh>().text = classesOrdered[i][0];
            LightEmUp(i, buttonAt[i]);
        }
        
        Debug.LogFormat("[Curriculum #{0}] Band Practice: {1}", _moduleId, bandPractice);
        Debug.LogFormat("[Curriculum #{0}] Sleepy Gary: {1}", _moduleId, sleepyGary);
    }

    void SubmitPressed() {
        //TODO : write the current situation whenever the Submit button is pressed
        
        String temp = "";
        for (int i = 0; i < 5; i++) {
            temp += classesOrdered[i][buttonAt[i]] + " - ";
        }
        Debug.LogFormat("[Curriculum #{0}] Submit pressed", _moduleId);
        Debug.LogFormat("[Curriculum #{0}] Current button states: {1}", _moduleId, temp.TrimEnd(' ', '-'));
        Debug.LogFormat("[Curriculum #{0}] Bookworm: {1}", _moduleId, Info.GetStrikes() == 0);


        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submit.transform);
        if (!CheckLectures()) {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Curriculum #{0}] Strike: Wrong classes taken", _moduleId);
            return;
        }
        bool bwCheck = true;
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 6; j++) {
                if (cells[i * 6 + j].activeInHierarchy)
                {
                    if ( ( bandPractice && (i == 0 || i == 2) && j > 2 ) || ( sleepyGary && j == 0 ) ) {
                        GetComponent<KMBombModule>().HandleStrike();
                        Debug.LogFormat("[Curriculum] Strike: Check conditions");
                        return;
                        
                    }
                    else if (cells[i * 6 + j].GetComponent<MeshRenderer>().material.color == Color.red) //zaa
                    {
                        if (Info.GetStrikes() == 0 && bwCheck)
                            bwCheck = false;
                        else
                        {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Curriculum] Strike: Too many conflicts");
                            return;
                        }
                    }
                }
            }
        }

        GetComponent<KMBombModule>().HandlePass();
       
    }

    private bool CheckLectures()
    {
        for (int i = 0; i < CorrectSections.Length; i++) {
            if (CorrectSections[i] > 2 && buttonAt[i] < 2) return false;
            else if (CorrectSections[i] < 2 && buttonAt[i] > 2) return false;
        }
        return true;
    }

    void ButtonPressed(int buttonNum){
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[buttonNum].transform);
        if (buttonAt[buttonNum] < 5)
        {
            buttonAt[buttonNum]++;
            buttons[buttonNum].GetComponentInChildren<TextMesh>().text = classesOrdered[buttonNum][buttonAt[buttonNum]];
            
        }
        else {
            buttons[buttonNum].GetComponentInChildren<TextMesh>().text = classesOrdered[buttonNum][0];
            buttonAt[buttonNum]=0;
        }
        LightEmUp(buttonNum, buttonAt[buttonNum]);
        
        
    }
}
