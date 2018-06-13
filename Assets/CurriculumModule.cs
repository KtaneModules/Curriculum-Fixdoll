using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Curriculum;
using System;
using System.Collections;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

enum Cond { BandPractice, SleepyGary, Mathlete, PartTimer, PartyAnimal, FreshmanYear, Nothing };//TODO everything you haven't done yet
public class ButtonState
{
    public bool[][] grid;
    public bool isEmpty = true;
    public int LectureCount = 0;


    public ButtonState()
    {
        grid = new bool[5][];
        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = new bool[6];
        }
        isEmpty = true;
        LectureCount = 0;
    }

    public ButtonState(bool[][] _grid)
    {
        isEmpty = true;
        LectureCount = 0;

        grid = _grid;
    }
}

[System.Serializable]
public class CurriculumModule : MonoBehaviour
{


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
    //bool sleepyGary;
    //bool bandPractice;
    string serial;
    Cond condition;


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
    void Start()
    {

        _moduleId = _moduleCount++;
        ModuleSetup();
        GetComponent<KMBombModule>().OnActivate += ModuleInit;
    }

    void ModuleSetup()
    {

        //Button interaction init
        submit.OnInteract = delegate () { SubmitPressed(); return false; };
        for (int i = 0; i < 5; i++)
        {
            int j = i;
            buttons[j].OnInteract = delegate () { ButtonPressed(j); return false; };
        }

        //Button random
        List<string[]> temp = new List<string[]>();
        for (int i = 0; i < 5; i++)
        {
            temp.Add(classes[i]);
        }
        for (int i = 4; i >= 0; i--)
        {
            int ind = UnityEngine.Random.Range(0, i + 1);
            classesOrdered[i] = temp.ElementAt(ind);
            temp.RemoveAt(ind);
        }


    }

    private void LightEmUp(int b, int s, bool dontCheckPrevious = false)
    {
        bool[][] temp;
        if (!dontCheckPrevious)
        {
            temp = Sections[b][(s + 5) % 6].grid;
            for (int i = 0; i < temp.Length; i++)
            {
                for (int j = 0; j < temp[i].Length; j++)
                {
                    if (temp[i][j])
                    {
                        if (cells[i * 6 + j].GetComponent<MeshRenderer>().material.color == Color.red)
                        {
                            var hold1 = Sections[(b + 1) % 5][buttonAt[(b + 1) % 5]].grid;
                            var hold2 = Sections[(b + 2) % 5][buttonAt[(b + 2) % 5]].grid;
                            var hold3 = Sections[(b + 3) % 5][buttonAt[(b + 3) % 5]].grid;
                            var hold4 = Sections[(b + 4) % 5][buttonAt[(b + 4) % 5]].grid;
                            var hold5 = (new List<bool>() { hold1[i][j], hold2[i][j], hold3[i][j], hold4[i][j] }).Where(x => x == true);
                            if (hold5.Count() < 2) cells[i * 6 + j].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.white);

                        }
                        else cells[i * 6 + j].SetActive(false);
                    }

                }
            }
        }
        temp = Sections[b][s].grid;
        for (int i = 0; i < temp.Length; i++)
        {
            for (int j = 0; j < temp[i].Length; j++)
            {
                if (temp[i][j])
                {
                    if (cells[i * 6 + j].activeInHierarchy)
                    {
                        cells[i * 6 + j].GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
                    }
                    else cells[i * 6 + j].SetActive(true);

                }
            }
        }
    }

    private void TurnOffBoard()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                cells[i * 6 + j].SetActive(false);
            }
        }
    }

    private void RandomizeLectureCounts()
    {
        int[] LectureCount = new int[10];
        for (int i = 0; i < LectureCount.Length; i++)
        {
            LectureCount[i] = Random.Range(2, 4);
        }
        for (int i = 0; i < 5; i++)
        {
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

    private void AssignCorrectSections(string serial)
    {
        for (int i = 0; i < 5; i++)
        {
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
        for (int i = 0; i < Sections.Length; i++)
        {
            for (int j = 0; j < Sections[i].Length; j++)
            {
                if (Sections[i][j].isEmpty)
                {
                    Stack<Vector2> lecs = new Stack<Vector2>();
                    for (int k = 0; k < 3; k++)
                    {
                        Vector2 temp = new Vector2(UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 6));
                        if (lecs.Contains(temp)) k--;
                        else lecs.Push(temp);
                    }
                    for (int k = 0; k < Sections[i][j].LectureCount; k++)
                    {
                        Vector2 temp = lecs.Pop();
                        Sections[i][j].grid[(int)temp.x][(int)temp.y] = true;
                    }
                    lecs.Clear();
                    Sections[i][j].isEmpty = false;
                }
            }
        }
    }

    private void GenerateSolutions(Cond condition)
    {
        Stack<Vector2> lecs = new Stack<Vector2>();
        for (int i = 0; i < 15; i++)
        {
            Vector2 temp = new Vector2(UnityEngine.Random.Range(0, 5), UnityEngine.Random.Range(0, 6));

            if (lecs.Contains(temp)) i--;
            else if (condition == Cond.Nothing) lecs.Push(temp);
            else if (condition == Cond.Mathlete && (int)temp.x == 1) i--;
            else if (condition == Cond.PartyAnimal && (int)temp.y == 5) i--;
            else if (condition == Cond.PartTimer && (int)temp.x > 2) i--;
            else if (condition == Cond.SleepyGary && (int)temp.y == 0) i--;
            else if (condition == Cond.BandPractice && ((int)temp.x == 0 || (int)temp.x == 2) && (int)temp.y > 2) i--;
            else if (condition == Cond.FreshmanYear && (int)temp.x == 4 && (int)temp.y > 2) i--;
            else lecs.Push(temp);

            /*if (bp && ((int)temp.x == 0 || (int)temp.x == 2) && (int)temp.y > 2) i--;
            else if (sg && (int)temp.y == 0) i--;
            else if (lecs.Contains(temp)) i--;
            else {
                lecs.Push(temp);
            }*/
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
        else
        {
            for (int i = 0; i < 5; i++)
            {
                int randSec = UnityEngine.Random.Range(0, 3);
                if (CorrectSections[i] < 3) randSec += 3;
                while (!Sections[i][randSec].isEmpty)
                {
                    randSec = UnityEngine.Random.Range(0, 3);
                    if (CorrectSections[i] < 3) randSec += 3;
                }
                for (int j = 0; j < Sections[i][randSec].LectureCount; j++)
                {
                    Vector2 temp = lecs.Pop();
                    Sections[i][randSec].grid[(int)temp.x][(int)temp.y] = true;
                    Sections[i][randSec].isEmpty = false;
                }

            }
        }
        lecs.Clear();
    }

    void ModuleInit()
    {


        bool emptyPlate = false;
        foreach (object[] plate in Info.GetPortPlates())
        {
            if (plate.Length == 0)
            {
                emptyPlate = true;
                break;
            }
        }

        serial = Info.GetSerialNumber();

        if (serial.EndsWith("0")) condition = Cond.Mathlete;
        else if (Info.GetPortCount() >= 5) condition = Cond.PartyAnimal;
        else if (emptyPlate) condition = Cond.PartTimer;
        else if (Info.GetIndicators().Count() == 0) condition = Cond.SleepyGary;
        else if (Info.GetBatteryCount() > 2) condition = Cond.BandPractice;
        else condition = Cond.FreshmanYear;

        Debug.LogFormat("[Curriculum #{0}] Condition: {1}", _moduleId, condition);

        TurnOffBoard();

        RandomizeLectureCounts();

        AssignCorrectSections(serial);

        Debug.LogFormat("[Curriculum #{0}] Classes on the buttons, in reading order (Asterisks indicate the correct class in each pair):", _moduleId);
        for (int i = 0; i < 5; i++)
        {
            Debug.LogFormat("[Curriculum #{0}] {1}", _moduleId, classPairNames[Array.IndexOf(classes, classesOrdered[i])]);
        }


        //Teh main solution
        GenerateSolutions(condition);
        //Generate multiple false solutions to avoid button mashing
        int FalseSolves = UnityEngine.Random.Range(1, 3);
        for (int i = 0; i < FalseSolves; i++)
        {
            GenerateSolutions(Cond.Nothing);
        }

        for (int i = 0; i < 5 - FalseSolves; i++)
        {
            GenerateOtherCycles();
        }

        //Button text & light init
        for (int i = 0; i < 5; i++)
        {
            buttons[i].GetComponentInChildren<TextMesh>().text = classesOrdered[i][0];
            LightEmUp(i, buttonAt[i], true);
        }

    }

    void SubmitPressed()
    {
        //TODO : write the current situation whenever the Submit button is pressed

        String temp = "";
        for (int i = 0; i < 5; i++)
        {
            temp += classesOrdered[i][buttonAt[i]] + " - ";
        }
        Debug.LogFormat("[Curriculum #{0}] Submit pressed", _moduleId);
        Debug.LogFormat("[Curriculum #{0}] Current button states: {1}", _moduleId, temp.TrimEnd(' ', '-'));
        Debug.LogFormat("[Curriculum #{0}] Bookworm: {1}", _moduleId, Bookworm());

        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submit.transform);
        if (!CheckLectures())
        {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Curriculum #{0}] Strike: Wrong classes taken", _moduleId);
            return;
        }
        bool bookwormChecked = false;
        switch (condition)
        {
            default: { break; }
            case Cond.Mathlete:
                {

                    for (int j = 0; j < 6; j++)
                    {
                        if (cells[6 + j].activeInHierarchy)
                        {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Curriculum #{0}] Strike: Check the condition", _moduleId);
                            return;
                        }
                    }
                    break;
                }
            case Cond.PartyAnimal:
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (cells[i * 6 + 5].activeInHierarchy)
                        {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Curriculum #{0}] Strike: Check the condition", _moduleId);
                            return;
                        }
                    }
                    break;
                }
            case Cond.PartTimer:
                {
                    for (int j = 0; j < 6; j++)
                    {
                        if (cells[18 + j].activeInHierarchy || cells[24 + j].activeInHierarchy)
                        {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Curriculum #{0}] Strike: Check the condition", _moduleId);
                            return;
                        }
                    }
                    break;
                }
            case Cond.BandPractice:
                {
                    for (int j = 3; j < 6; j++)
                    {
                        if (cells[0 + j].activeInHierarchy || cells[12 + j].activeInHierarchy)
                        {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Curriculum #{0}] Strike: Check the condition", _moduleId);
                            return;
                        }
                    }
                    break;
                }
            case Cond.SleepyGary:
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (cells[i * 6 + 0].activeInHierarchy)
                        {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Curriculum #{0}] Strike: Check the condition", _moduleId);
                            return;
                        }
                    }
                    break;
                }
            case Cond.FreshmanYear:
                {
                    for (int j = 3; j < 6; j++)
                    {
                        if (cells[24 + j].activeInHierarchy)
                        {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Curriculum #{0}] Strike: Check the condition", _moduleId);
                            return;
                        }
                    }
                    break;
                }
        }
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (cells[i * 6 + j].activeInHierarchy)
                {
                    if (cells[i * 6 + j].GetComponent<MeshRenderer>().material.color == Color.red) //lol
                    {
                        if (Bookworm() && !bookwormChecked)
                            bookwormChecked = true;
                        else
                        {
                            GetComponent<KMBombModule>().HandleStrike();
                            Debug.LogFormat("[Curriculum #{0}] Strike: Too many conflicts", _moduleId);
                            return;
                        }
                    }
                }
            }
        }

        GetComponent<KMBombModule>().HandlePass();

    }

    private bool Bookworm()
    {
        if (Info.GetSolvedModuleNames().Count() > 0 && Info.GetStrikes() == 0) return true;
        return false;
    }

    private bool CheckLectures()
    {
        for (int i = 0; i < CorrectSections.Length; i++)
        {
            if (CorrectSections[i] > 2 && buttonAt[i] <= 2) return false;
            else if (CorrectSections[i] <= 2 && buttonAt[i] > 2) return false;
        }
        return true;
    }

    void ButtonPressed(int buttonNum)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[buttonNum].transform);
        if (buttonAt[buttonNum] < 5)
        {
            buttonAt[buttonNum]++;
            buttons[buttonNum].GetComponentInChildren<TextMesh>().text = classesOrdered[buttonNum][buttonAt[buttonNum]];

        }
        else
        {
            buttons[buttonNum].GetComponentInChildren<TextMesh>().text = classesOrdered[buttonNum][0];
            buttonAt[buttonNum] = 0;
        }
        LightEmUp(buttonNum, buttonAt[buttonNum]);
    }

    private bool TwitchShouldCancelCommand;
    private string TwitchHelpMessage = "Cycle the buttons !{0} cycle. Toggle all the classes with !{0} toggle. Toggle multiple classes with !{0} toggle 1 3 4. Click a button using !{0} click 2. It's possible to add a number of times to click: !{0} click 2 3. Buttons are numbered left to right. Submit your answer with !{0} submit.";

    private IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        while (string.IsNullOrEmpty(serial)) yield return true;

        int[] buttonInts = new[] { 0, 1, 2, 3, 4 };
        while (buttonInts.Any(x => buttonAt[x] != CorrectSections[x]))
        {
            buttons[buttonInts.First(x => buttonAt[x] != CorrectSections[x])].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }

        submit.OnInteract();
        yield return new WaitForSeconds(0.1f);
    }

    int[] buttonOffset = new int[6] { 0, 0, 0, 0, 0, 0 };
    private IEnumerator ProcessTwitchCommand(string inputCommand)
    {
        var commands = inputCommand.ToLowerInvariant().Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (commands.Length.InRange(2, 3) && commands[0].EqualsAny("click", "press"))
        {
            int buttonPosition;
            if (int.TryParse(commands[1], out buttonPosition))
            {
                if (!buttonPosition.InRange(1, 5)) yield break;

                int clicks = 1;
                if (commands.Length == 3 && !int.TryParse(commands[2], out clicks))
                {
                    yield break;
                }

                clicks %= 6;

                if (clicks == 0) yield break;

                yield return null;

                buttonPosition -= 1;

                KMSelectable button = buttons[buttonPosition];
                for (int i = 0; i < clicks; i++)
                {
                    button.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }

                buttonOffset[buttonPosition] += clicks;
                buttonOffset[buttonPosition] %= 6;
            }
        }
        else if (commands.Length == 1 && commands[0] == "submit")
        {
            yield return null;

            submit.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        else if (commands.Length == 1 && commands[0] == "reset")
        {
            yield return null;
            for (int buttonPosition = 0; buttonPosition < 5; buttonPosition++)
            {
                KMSelectable button = buttons[buttonPosition];
                if (buttonOffset[buttonPosition] <= 0) continue;
                for (int i = 0; i < 6 - buttonOffset[buttonPosition]; i++)
                {
                    button.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                buttonOffset[buttonPosition] = 0;
            }
        }
        else if (commands.Length == 1 && commands[0] == "cycle")
        {
            for (int buttonPosition = 0; buttonPosition < 5 && !TwitchShouldCancelCommand; buttonPosition++)
            {
                yield return null;

                KMSelectable button = buttons[buttonPosition];
                for (int i2 = 0; i2 < 2; i2++)
                {
                    for (int i3 = 0; i3 < 15 && !TwitchShouldCancelCommand; i3++)
                    {
                        yield return new WaitForSeconds(0.1f);
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        button.OnInteract();
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }

            if (TwitchShouldCancelCommand)
                yield return "cancelled";
        }
        else if (commands[0].EqualsAny("toggle", "flip", "switch"))
        {
            int pos;
            if (commands.Length > 1 && commands.Skip(1).Any(x => !int.TryParse(x, out pos) || !pos.InRange(1, 5))) yield break;
            int[] buttonPositions = commands.Length == 1 ? new[] { 1, 2, 3, 4, 5 } : commands.Skip(1).Select(int.Parse).Distinct().ToArray();

            yield return null;
            foreach (int buttonPosition in buttonPositions)
            {
                KMSelectable button = buttons[buttonPosition - 1];
                for (int i = 0; i < 3; i++)
                {
                    button.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }

                buttonOffset[buttonPosition - 1] += 3;
                buttonOffset[buttonPosition - 1] %= 6;
            }
        }
    }
}