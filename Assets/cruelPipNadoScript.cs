using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class cruelPipNadoScript : MonoBehaviour {

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;

	public KMSelectable[] textButtons;
	public KMSelectable topHat;

	public TextMesh mainDisplay;
	public TextMesh timerDisplay;
	public TextMesh[] subDisplays;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	bool countdownTimer;

	private string[] regularNatoAlphabet = { "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel", "India", "Juliet", "Kilo", "Lima", "Mike", "November", "Oscar", "Papa", "Quebec", "Romeo", "Sierra", "Tango", "Uniform", "Victor", "Whiskey", "XRay", "Yankee", "Zulu" };
	private string[] pipNadoAlphabet = { "Alpha", "Beta", "Crab", "D---", "Echo", "F---", "Glubbers_", "Hotel", "Igloo", "J", "Kilogram", "Lemon", "Margherita", "Nostril", "Octopuuus", "Poggers", "Quack!", "Ronald Reagan", "Sunscreen", "Terraforming", "Umbrella", "Vi-o-lin", "Wisconsin", "Xbox", "YEET", "Zuckerburg" };
	private string alphabetReference = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	private int offset;
	private string regularNatoString;
	private int[] dispNumbers = new int[3];
	private int[] initalNumbers = new int[3];
	private int calculatedNumber = 0;
	int stage = 0;
	private int[] pipAmount;
	int letterSelection = 0;
	int[] letterLength;
	char[] letters;
	string[] solveText = { "smhile", "Good job!", "Nicely done!", "Excellent!", "Solved!", "The end?", "You did it!", "Amazing!", "Yay!" };

	void Awake()
    {

		moduleId = moduleIdCounter++;

		foreach (KMSelectable button in textButtons)
		{
			button.OnInteract += delegate () { subButtonPress(button); return false; };
		}

		topHat.OnInteract += delegate () { topHatPress(); return false; };

	}

	
	void Start()
    {
		mainDisplay.text = "";
		timerDisplay.text = "";

		for (int i = 0; i < 3; i++)
        {
			subDisplays[i].text = "";
        }

		textSelection();
		calcOffset();
    }

	void textSelection()
    {
		letterSelection = rnd.Range(0, regularNatoAlphabet.Count());

		regularNatoString = regularNatoAlphabet[letterSelection];

		letterLength = new int[regularNatoString.Length];
		letters = regularNatoString.ToUpper().ToCharArray();


		for (int i = 0; i < letterLength.Length; i++)
        {
            letterLength[i] = alphabetReference.IndexOf(letters[i]);
        }

    }

	void subButtonPress(KMSelectable button)
    {
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		button.AddInteractionPunch();
		if (moduleSolved || !countdownTimer)
        {
			return;
        }

		for (int i = 0; i < 3; i++)
        {
			if (button == textButtons[i])
            {
				dispNumbers[i]++;
				if (dispNumbers[i] > 9)
                {
					dispNumbers[i] = 0;
                }
				subDisplays[i].text = dispNumbers[i].ToString();
            }
        }
    }

	void topHatPress()
    {
		topHat.AddInteractionPunch();
		if (moduleSolved)
        {
			return;
        }

		if (!countdownTimer && stage != 0) stage = 0;

        switch (stage)
        {
			case 0:
				if (!countdownTimer)
                {
					countdownTimer = true;
					StartCoroutine(displaySequence());
					StartCoroutine(countDownTimer());
					initSubDisplay();
					calcMain();
					stage++;
                }
				break;
			case 1:
				if (countdownTimer)
                {
					StopAllCoroutines();
					checkNumber();
                }
				break;
        }
    }

	void initSubDisplay()
    {
		for (int i = 0; i < 3; i++)
        {
			initalNumbers[i] = rnd.Range(0, 10);
			dispNumbers[i] = initalNumbers[i];
			subDisplays[i].text = dispNumbers[i].ToString();
        }
		Debug.LogFormat("[Cruel Pip-Nado #{0}] The initial number is {1}.", moduleId, int.Parse(initalNumbers[0].ToString() + initalNumbers[1].ToString() + initalNumbers[2].ToString()));
    }

	IEnumerator countDownTimer()
    {
		int seconds = 90;
		
		while (seconds != 0)
        {
			timerDisplay.text = seconds.ToString();
			seconds--;
			yield return new WaitForSeconds(1);
        }

		if (seconds == 0)
        {
			timerDisplay.text = "";
			strikeMessage("Time has run out!");
        }

		yield return null;
    }


	IEnumerator displaySequence()
    {
		int displayIndex = 0;
		int addLength = 0;

		Debug.LogFormat("[Cruel Pip-Nado #{0}] The timer has started.", moduleId);

		List<string> letterStuff = new List<string>();
		while (letterStuff.Count != letterLength.Count())
        {
			letterStuff.Add(pipNadoAlphabet[letterLength[addLength]]);
			addLength++;
        }

		Debug.LogFormat("[Cruel Pip-Nado #{0}] The displayed letters are: {1}. The first letters of each word spells out {2}.", moduleId, letterStuff.Join(", "), regularNatoAlphabet[letterSelection]);

		while (countdownTimer)
        {
			if (displayIndex == letterLength.Length) displayIndex = 0;

			mainDisplay.text = pipNadoAlphabet[letterLength[displayIndex]];
			yield return new WaitForSeconds(0.4f);
			mainDisplay.text = "";
			displayIndex++;
			yield return new WaitForSeconds(0.4f);
        }
		
	

		yield return null;
    }

	private bool isEven(int number)
    {
		return number % 2 == 0;
    }

	void calcOffset()
    {
		offset = int.Parse(Bomb.GetSerialNumberNumbers().First().ToString() + Bomb.GetSerialNumberNumbers().Last().ToString());
		offset = offset * Bomb.GetModuleNames().Count();
		Debug.LogFormat("[Cruel Pip-Nado #{0}] The initial offset after concatenating the first and last digits of the serial number, multiplied by the number of modules is {1}.", moduleId, offset);
		if (isEven(offset))
        {
			offset = offset / 2;
			Debug.LogFormat("[Cruel Pip-Nado #{0}] After obtaining the inital offset, the number is even. Dividing by 2. The current offset number now is {1}.", moduleId, offset);
        }
        else
        {
			offset = offset * 2;
			Debug.LogFormat("[Cruel Pip-Nado #{0}] After obtaining the inital offset, the number is odd. Multiplying by 2. The current offset number now is {1}.", moduleId, offset);
		}
		if (Bomb.GetIndicators().Count() + Bomb.GetPortCount() + Bomb.GetPortPlateCount() == 0)
        {
			Debug.LogFormat("[Cruel Pip-Nado #{0}] There are no indicators, ports, and port plates. Leaving the offset as is.", moduleId);
        }
        else
        {
			offset = offset * (Bomb.GetIndicators().Count() + Bomb.GetPortCount() + Bomb.GetPortPlateCount());
			Debug.LogFormat("[Cruel Pip-Nado #{0}] After multiplying the offset by the sum of indicators, ports, and port plates, the current offset number now is {1}.", moduleId, offset);
		}
		
		if (alphabetReference.IndexOf(Bomb.GetSerialNumberLetters().First()) > Bomb.GetSerialNumberNumbers().Sum())
        {
			offset = offset + Bomb.GetSerialNumberLetters().Count(x => x == 'A' || x == 'E' || x == 'I' || x == 'O' || x == 'U');
			Debug.LogFormat("[Cruel Pip-Nado #{0}] The alphabetical position (zero-indexed) of the first letter in the serial number is greater than the sum of all digits in the serial number. After adding the number of vowels, the current offset number now is {1}.", moduleId, offset);
        }
        else
        {
			offset = offset + Bomb.GetSerialNumberLetters().Count(x => x != 'A' && x != 'E' && x != 'I' && x != 'O' && x != 'U');
			Debug.LogFormat("[Cruel Pip-Nado #{0}] The alphabetical position (zero-indexed) of the first letter in the serial number is not greater than the sum of all digits in the serial number. After adding the number of consonants, the current offset number now is {1}.", moduleId, offset); ;
        }



		offset = offset % 100;

		Debug.LogFormat("[Cruel Pip-Nado #{0}] The final offset after moduloing 100 is {1}.", moduleId, offset);
	
    }

	void calcMain()
    {
		int initNumber = int.Parse(dispNumbers[0].ToString() + dispNumbers[1].ToString() + dispNumbers[2].ToString());

		if (letterSelection == 0)
        {
			Debug.LogFormat("[Cruel Pip-Nado #{0}] The alphabetical position is A. Leave the initial number as is.", moduleId);
        }
        else
        {
			initNumber = initNumber * letterSelection;

			Debug.LogFormat("[Cruel Pip-Nado #{0}] The selected letter is {1}. After multiplying the initial number by {2}, the modified intial number now is {3}.", moduleId, alphabetReference[letterSelection], letterSelection, initNumber);

		}



		pipAmount = new int[letterLength.Count()];

		for (int i = 0; i < letterLength.Count(); i++)
        {
			pipAmount[i] = pipNadoAlphabet[letterLength[i]].Length;
        }

		int calculatedLengths = pipAmount.Sum();

		Debug.LogFormat("[Cruel Pip-Nado #{0}] The calculated letter lengths of {1} = {2}.", moduleId, pipAmount.Join(" + "), calculatedLengths);

		calculatedNumber = ((calculatedLengths + initNumber) * offset) % 1000;

		if (Bomb.GetSerialNumberLetters().Any(x => x == 'P' || x == 'I' || x == 'Y'))
        {
			int num = calculatedNumber;
			int remainder, reverse = 0;

			while (num > 0)
            {
				remainder = num % 10;
				reverse = reverse * 10 + remainder;
				num /= 10;
            }
			calculatedNumber = reverse;
			Debug.LogFormat("[Cruel Pip-Nado #{0}] The letters in the serial number contains \"PIPPY\". Reverse the number.", moduleId);
        }
        else
        {
			Debug.LogFormat("[Cruel Pip-Nado #{0}] The letters in the serial number doesn't contain \"PIPPY\". Don't reverse the number.", moduleId);
        }

		Debug.LogFormat("[Cruel Pip-Nado #{0}] The final number is {1}.", moduleId, calculatedNumber);
    }

	void checkNumber()
    {
		countdownTimer = false;

		int finalNumber = int.Parse(dispNumbers[0].ToString() + dispNumbers[1].ToString() + dispNumbers[2].ToString());

		if (finalNumber == calculatedNumber)
        {
			StartCoroutine(solveStuff());
        }
        else
        {
			string strike = "Expected " + calculatedNumber.ToString() + ", but inputted " + finalNumber.ToString() + ".";
			strikeMessage(strike);
        }
    }

	IEnumerator solveStuff()
    {
		Debug.LogFormat("[Cruel Pip-Nado #{0}] That is correct! Module solved!", moduleId);
		moduleSolved = true;
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
		GetComponent<KMBombModule>().HandlePass();
		
		for (int i = 0; i < 3; i++)
        {
			subDisplays[i].text = "";
        }
		timerDisplay.text = "";
		mainDisplay.text = solveText.PickRandom();
		yield return new WaitForSeconds(1.5f);
		mainDisplay.text = "";

		yield return null;
    }

	void strikeMessage(string reason)
    {
		for (int i = 0; i < 3; i++)
        {
			subDisplays[i].text = "";
        }
		mainDisplay.text = "";
		timerDisplay.text = "";
		countdownTimer = false;
		GetComponent<KMBombModule>().HandleStrike();
		Debug.LogFormat("[Cruel Pip-Nado #{0}] Strike! {1} Resetting...", moduleId, reason);
		textSelection();

    }

	
	
	void Update()
    {

    }

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Use !{0} start to initialize the module. | !{0} submit 0-9 to input your number. Make sure to use spaces when submitting.";
#pragma warning restore 414

	private bool validNumbers(string c)
    {
		string[] valids = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
		if (!valids.Contains(c))
        {
			return false;
        }
		return true;
    }


	IEnumerator ProcessTwitchCommand (string command)
    {
		string[] commands = command.ToUpper().Split(' ');
		yield return null;

		if (commands[0].Equals("START"))
        {
			if (!countdownTimer)
            {
				topHat.OnInteract();
            }
			else if (countdownTimer)
            {
				yield return "sendtochaterror The timer has already started!";
            }
			yield break;
        }

		if (commands[0].Equals("SUBMIT"))
        {
			if (!countdownTimer)
            {
				yield return "sendtochaterror You haven't started the module yet!";
				yield break;
            }
			if (commands.Length > 4)
            {
				yield return "sendtochaterror You can't submit over 3 numbers!";
            }
			else if (commands.Length == 4)
            {
				if (validNumbers(commands[1]))
                {
					if (validNumbers(commands[2]))
                    {
						if (validNumbers(commands[3]))
                        {
							int temp1 = 0;
							int temp2 = 0;
							int temp3 = 0;
							int.TryParse(commands[1], out temp1);
							int.TryParse(commands[2], out temp2);
							int.TryParse(commands[3], out temp3);

							while (temp1 != dispNumbers[0])
                            {
								textButtons[0].OnInteract();
								yield return new WaitForSeconds(0.1f);
                            }
							while (temp2 != dispNumbers[1])
                            {
								textButtons[1].OnInteract();
								yield return new WaitForSeconds(0.1f);
                            }
							while (temp3 != dispNumbers[2])
                            {
								textButtons[2].OnInteract();
								yield return new WaitForSeconds(0.1f);
                            }

							topHat.OnInteract();
                        }
                        else
                        {
							yield return "sendtochaterror '" + commands[3] + "' is an invalid value!";
                        }
                    }
                    else
                    {
						yield return "sendtochaterror '" + commands[2] + "' is an invalid value!";
                    }
                }
                else
                {
					yield return "sendtochaterror '" + commands[1] + "' is an invalid value!";
                }
            }
			else if (commands.Length == 3)
            {
				yield return "sendtochaterror Please specify the value of the third number!";
            }
			else if (commands.Length == 2)
            {
				yield return "sendtochaterror Please specify the values of the second and third numbers!";
            }
			else if (commands.Length == 1)
            {
				yield return "sendtochaterror Please specify the values of all three numbers!";
            }

			yield break;
        }
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;

		if (!countdownTimer)
        {
			topHat.OnInteract();
        }

		int temp1 = calculatedNumber / 100;
		int temp2 = (calculatedNumber / 10) % 10;
		int temp3 = calculatedNumber % 10;

		while (temp1 != dispNumbers[0])
        {
			textButtons[0].OnInteract();
			yield return new WaitForSeconds(0.1f);
        }
		while (temp2 != dispNumbers[1])
        {
			textButtons[1].OnInteract();
			yield return new WaitForSeconds(0.1f);
        }
		while (temp3 != dispNumbers[2])
        {
			textButtons[2].OnInteract();
			yield return new WaitForSeconds(0.1f);
        }
		topHat.OnInteract();
    }


}





