using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;

[RequireComponent(typeof(AudioSource))]
public class GameController : MonoBehaviour {
	
	[Header("---Canvases---")]

	[Tooltip("The Menu Canvas")]
	public GameObject MenuCanvas;
	[Tooltip("The Category Canvas")]
	public GameObject CategoryCanvas;
	[Tooltip("The Game Canvas")]
	public GameObject GameCanvas;
	[Tooltip("The Head to Head Canvas")]
	public GameObject H2HCanvas;
	[Tooltip("The Pause Canvas")]
	public GameObject PauseCanvas;
	[Tooltip("The GameOver Canvas")]
	public GameObject GameOverCanvas;


	[Header("---Single Player UI---")]

	[Tooltip("Parent GameObject that holds the true and false buttons")]
	public CanvasGroup Buttons;
	[Tooltip("Text component used to display the question")]
	public Text QuestionDisplay;
	[Tooltip("Text component used to display result of the question answered. i.e. Correct or Incorrect")]
	public Text ResultDisplay;
	[Tooltip("Text component used to display the time left to answer a question")]
	public Text Timer;
	[Tooltip("Text component used to display the current score")]
	public Text ScoreText;
	[Tooltip("Text component used to display the number of questions answered")]
	public Text QuizCounter;
	[Tooltip("Pictorial representation of the time")]
	public Image TimerFill;
	[Tooltip("Text component used to display the score on GameOver")]
	public Text FinalScore;
	[Tooltip("Text component used to display the highscore on GameOver")]
	public Text CurHighscore;
	[Tooltip("An array that holds the 'heart/lives' images")]
	public Image[] Lives;


	[Header("---Menu UI---")]

	[Tooltip("The sound button image")]
	public Image SoundButton;
	[Tooltip("The sound button text")]
	public Text SoundText;
	[Tooltip("The category button parent")]
	public GameObject Categories;
	[Tooltip("The loading graphic")]
	public GameObject loading;


	[Header("---H2H UI---")]

	[Tooltip("Text component used to display the question")]
	public Text[] QuestionDisplays;
	[Tooltip("Parent GameObject that holds the true and false buttons")]
	public CanvasGroup ButtonsPlayer1;
	[Tooltip("Parent GameObject that holds the true and false buttons")]
	public CanvasGroup ButtonsPlayer2;
	[Tooltip("Text component used to display the current score")]
	public Text ScoreTextP1;
	[Tooltip("Text component used to display the current score")]
	public Text ScoreTextP2;
	[Tooltip("The first to get this score wins")]
	public int WinningScore;

	[Header("---Game Components---")]

	[Tooltip("The image to display when the game sound is muted")]
	public Sprite MuteGraphic;
	[Tooltip("The image to display when the game sound is ON")]
	public Sprite SoundOnGraphic;
	[Tooltip("An array that holds the sound effects")]
	public AudioClip[] SoundEffect;
	[Tooltip("A list of colors we will use throughout the game")]
	public Color[] ColorList;
	//[HideInInspector]
	public List<ToF> Questions;


	//Number of available lives minus one since arrays start from zero and not one.
	//For example an array with 5 lives will display as (0,1,2,3,4)
	private int AvailableLives = 4;
	//Currently displayed question
	private int currentQuiz = 0;
	//Currently displayed question in H2H
	private int currentH2HQuiz = 0;
	//Number of questions answered correctly
	private int CorrectAnswer = 0;
	//The text that appears before the score. ex: 'Score'
	private string Prefix = "Score: ";
	//Countdown timer for a question
	private float TimeLeft = 15;
	//The floats we use to keep track of the score
	private float score = 0;
	private float scoreCount = 0;
	//The int we use to keep track of the player 1's score
	private int ScoreP1=0;
	//The int we use to keep track of the player 2's score
	private int ScoreP2=0;
	//The float we use to keep track of the highscore
	private float highscore = 0;
	//A bool we will use to restart the timer incase the time is over
	private bool TimeOver = false;
	//A bool we will use to check if the game has started
	private bool Started = false;
	//A bool we will use to check if the game is over
	private bool GameOver = true;
	//A bool we will use to check if the game is paused
	private bool Paused = false;
	//A bool we will use to check if the user answered the question
	private bool Answered = false;
	//A bool we will use to check if the user entered answer in H2H mode
	private bool EnteredAns;
	//A bool we will use to check if the category canvas is active
	private bool CategoryShow = false;
	//A bool we will use to check if the selected mode is H2H
	private bool H2HMode = false;
	//A general use index
	private int index = 0;
	//Audiosource identifier
	private AudioSource SoundFX;
	//An int we will use to store the sound state (Sound off or Sound on)
	private int Sound = 0;
	//A bool we will use to check if the sound ticking effect is playing
	private bool ticking = false;
	//The path to the xml file we are loading.
	private string path;

	// Use this for initialization
	void Start () 
	{
		//Cache the audiosource component
		SoundFX=GetComponent<AudioSource>();

		//Get the sound state( 1 - Sound on. 0 - Sound off)
		Sound=PlayerPrefs.GetInt("Sound",1);

		//Update the sound settings
		if(Sound==0)
		{
			AudioListener.pause=true;
			SoundButton.sprite=MuteGraphic;
			SoundText.text="Sound OFF";
		}
		else if (Sound==1)
		{
			AudioListener.pause=false;
			SoundButton.sprite=SoundOnGraphic;
			SoundText.text="Sound ON";
		}
		else
		{
			PlayerPrefs.SetInt("Sound",1);
			AudioListener.pause=false;
			SoundButton.sprite=SoundOnGraphic;
			SoundText.text="Sound ON";
		}

		//Disable all other canvases
		if(GameCanvas) GameCanvas.SetActive(false);
		if(H2HCanvas) H2HCanvas.SetActive(false);
		if(CategoryCanvas) CategoryCanvas.SetActive(false);
		if(GameOverCanvas) GameOverCanvas.SetActive(false);
		if(PauseCanvas) PauseCanvas.SetActive(false);
		//Enable menu Canvas
		if(MenuCanvas) MenuCanvas.SetActive(true);
	}

	void  Update()
	{
		if(!H2HMode)
		{
			// Make the score count up to its current value
			if ( scoreCount > score )
			{
				// Count up to the courrent value
				score = Mathf.Lerp( score, scoreCount, 0.15f);

				// Update the score text
				UpdateScore();
			}
		}

		//Pause or resume when the back key/escape is pressed
		if(Input.GetKeyDown(KeyCode.Escape) && Started==true && GameOver==false && Paused==false)
			PauseGame();
		else if(Input.GetKeyDown(KeyCode.Escape) && Started==true && GameOver==false && Paused==true)
			ResumeGame();

		if(Input.GetKeyDown(KeyCode.Escape) && CategoryShow==true)
			HideCategory();
		else if(Input.GetKeyDown(KeyCode.Escape) && Started==false && GameOver==true && Paused==false && CategoryShow==false)
			Application.Quit();
	}

	public void ToggleSound()
	{
		//Get sound state
		Sound=PlayerPrefs.GetInt("Sound",1);

		//Mute or unmute depending on the current state
		if(Sound==0)
		{
			AudioListener.pause=false;
			PlayerPrefs.SetInt("Sound",1);
			SoundButton.sprite=SoundOnGraphic;
			SoundText.text="Sound ON";
		}
		else if (Sound==1)
		{
			AudioListener.pause=true;
			PlayerPrefs.SetInt("Sound",0);
			SoundButton.sprite=MuteGraphic;
			SoundText.text="Sound OFF";
		}	

		SoundFX.Stop();
	}

	//Hide Category Canvas
	public void HideCategory()
	{
		CategoryCanvas.SetActive(false);
		MenuCanvas.SetActive(true);
		CategoryShow=false;
	}

	//Start new game
	public void PlayGame()
	{
		Invoke("StartGame",0.75f);
		H2HMode=false;
	}

	public void PlayH2HGame()
	{
		Invoke("StartGame",0.75f);
		H2HMode=true;
	}

	void StartGame()
	{
		//Enable the category buttons
		if(Categories) Categories.SetActive(true);

		//Hide loading animation
		if(loading) loading.SetActive(false);

		//Disable menu canvas
		MenuCanvas.SetActive(false);

		//Display category canvas
		CategoryCanvas.SetActive(true);

		//Set the category check bool to true
		CategoryShow=true;

	}

	//Call this to select the category. Remember to set the XML path on the button eg XML/World(no quotes just plain text)
	public void CategorySelection(string path)
	{
		//Set the category check bool to false
		CategoryShow=false;

		//Set the "Started" bool to true
		Started=true;

		StartCoroutine(LoadQuestions(path));
		Categories.SetActive(false);
		loading.SetActive(true);
		loading.GetComponent<Animation>().Play();
	}

	//Load the question from the xml to the questions array
	IEnumerator LoadQuestions(string category)
	{
		//load the questions from the XML
		ToFContainer ic = ToFContainer.Load(category);

		//Clear List<Quiz> contents
		if(Questions.Count>0) Questions.Clear();

		//Parse the questions from the XML file to the array 
		foreach (ToF q in ic.Quizes)
			Questions.Add(q);

		yield return new WaitForSeconds(1);

		if(!H2HMode)
		{	
			//Initialize new game
			PrepareNewGame();

			//Displays the first question on the list
			NextQuestion();

			//Set gameOver bool to false
			GameOver=false;

			//Start countdown timer
			StartCoroutine(UpdateTimer());
		}
		else
		{
			//Initialize new game
			PrepareNewH2HGame();

			//Displays the first question on the list
			NextH2HQuestion();

			//Set gameOver bool to false
			GameOver=false;
		}
	}

	//Shuffles the question list
	void ShuffleQuestions()
	{
		// Go through all the questions and shuffle them
		for ( index = 0 ; index < Questions.Count ; index++ )
		{
			// Hold the questions in a temporary variable
			ToF tempNumber = Questions[index];

			// Choose a random index from the text list
			int randomIndex = Random.Range( index, Questions.Count);

			// Assign a random text from the list
			Questions[index] = Questions[randomIndex];

			// Assign the temporary text to the random question we chose
			Questions[randomIndex] = tempNumber;
		}
	}

	//Receives the answer chosen by the user in Single Player Mode. Either True or False.
	public void Answer(bool Value)
	{
		//Set "Answered" bool to true
		Answered=true;

		//Disable the buttons to prevent further user input
		Buttons.interactable=false;

		//Active the result display text
		ResultDisplay.gameObject.SetActive(true);

		//Check if the answer entered is correct or not and display the result
		if(Value==Questions[currentQuiz].isTrue)
		{
			//Check if the audiosource is playing a sound effect, stop it and play new effect. Otherwise just play the effect
			if(SoundFX.isPlaying)
			{
				SoundFX.Stop();
				SoundFX.PlayOneShot(SoundEffect[0]);
			}
			else
				SoundFX.PlayOneShot(SoundEffect[0]);

			SetScore();
			CorrectAnswer +=1;
			ResultDisplay.GetComponent<Text>().color=ColorList[1];
			ResultDisplay.text="CORRECT";

			//Update the correct answer counter
			UpdateCounter();

			//Increment the current question counter if we have more questions
			if(currentQuiz<Questions.Count-1) currentQuiz +=1;

			//Display the next question in x seconds
			Invoke("NextQuestion", 1.5f);
		}
		else
		{
			//Check if the audiosource is playing a sound effect, stop it and play new effect. Otherwise just play the effect
			if(SoundFX.isPlaying)
			{
				SoundFX.Stop();
				SoundFX.PlayOneShot(SoundEffect[1]);
			}
			else
				SoundFX.PlayOneShot(SoundEffect[1]);

			if(AvailableLives>=0) Lives[AvailableLives].color=ColorList[2];
			ResultDisplay.GetComponent<Text>().color=ColorList[0];
			ResultDisplay.text="INCORRECT";
			if(AvailableLives>=0) AvailableLives -=1;

			//Hide the ResultDisplay text so that we can show the correct answer.
			StartCoroutine(HideResultText());

			//Show the correct answer
			QuestionDisplay.text=Questions[currentQuiz].fact;

			//Check if we have available lives remaining
			Invoke("checkLives", 2.75f);

			//Update the correct answer counter
			UpdateCounter();

			//Increment the current question counter if we have more questions
			if(currentQuiz<Questions.Count-1) currentQuiz +=1;

			//Display the next question in x seconds
			Invoke("NextQuestion", 3f);
		}

	}

	//Receives the answer chosen by the user in H2H Mode.
	//We will use ints to inteprete the answer
	//Use the Key below
	//Int 1 is Player 1 has answered True
	//Int 2 is Player 1 has answered False
	//Int 3 is Player 2 has answered True
	//Int 4 is Player 2 has answered False
	public void AnswerH2H(int Ans)
	{
		//Disable the buttons to prevent further user input
		ButtonsPlayer1.interactable=false;
		ButtonsPlayer2.interactable=false;

		if(Ans==1 || Ans==3)
			EnteredAns=true;
		if(Ans==2 || Ans==4)
			EnteredAns=false;

		//Check if the answer entered is correct or not and display the result
		if(EnteredAns==Questions[currentH2HQuiz].isTrue)
		{
			//Check if the audiosource is playing a sound effect, stop it and play new effect. Otherwise just play the effect
			if(SoundFX.isPlaying)
			{
				SoundFX.Stop();
				SoundFX.PlayOneShot(SoundEffect[0]);
			}
			else
				SoundFX.PlayOneShot(SoundEffect[0]);

			//Update the scores
			if(Ans==1 || Ans==2)
				UpdateH2HCounter(1);
			else
				UpdateH2HCounter(2);
			
			//Increment the current question counter if we have more questions
			if(currentH2HQuiz<Questions.Count-1) currentH2HQuiz +=1;

			//Check if we have a winner in x seconds
			Invoke("CheckWinner", 1f);

			//Display the next question in x seconds
			Invoke("NextH2HQuestion", 1.5f);

		}
		else
		{
			if(SoundFX.isPlaying)
			{
				SoundFX.Stop();
				SoundFX.PlayOneShot(SoundEffect[1]);
			}
			else
				SoundFX.PlayOneShot(SoundEffect[1]);
			
			//Displays the correct answer
			for (int q=0;q<QuestionDisplays.Length;q++)
			{
				QuestionDisplays[q].text=Questions[currentH2HQuiz].fact;
			}

			//Update the scores
			if(Ans==1 || Ans==2)
				UpdateH2HCounter(3);
			else
				UpdateH2HCounter(4);

			//Increment the current question counter if we have more questions
			if(currentH2HQuiz<Questions.Count-1) currentH2HQuiz +=1;

			//Check if we have a winner in x seconds
			Invoke("CheckWinner", 2.75f);

			//Display the next question in x seconds
			Invoke("NextH2HQuestion", 3f);
		}
	}

	IEnumerator HideResultText()
	{
		yield return new WaitForSeconds(0.5f);
		ResultDisplay.gameObject.SetActive(false);
	}

	//Set the score based on the difficulty of the question
	void SetScore()
	{
		if(Questions[currentQuiz].difficulty==Difficulty.Easy)
			scoreCount +=10;
		if(Questions[currentQuiz].difficulty==Difficulty.Medium)
			scoreCount +=25;
		if(Questions[currentQuiz].difficulty==Difficulty.Hard)
			scoreCount +=50;
		if(Questions[currentQuiz].difficulty==Difficulty.Bonus)
			scoreCount +=100;
	}

	//Displays the current score on UI
	void UpdateScore()
	{
		ScoreText.text =Prefix + Mathf.CeilToInt(score).ToString();
	}

	//Updates the correct answer counter
	void UpdateCounter()
	{
		//Update the quiz counter text
		int a = currentQuiz+1;
		QuizCounter.text= CorrectAnswer.ToString() + " / " + a.ToString();
	}

	//Updates the H2H score counter
	void UpdateH2HCounter(int a)
	{
		if(a==1 || a==4)
		{
			ScoreP1 +=1;
			ScoreTextP1.text= ScoreP1.ToString() + " : " + ScoreP2.ToString();
			ScoreTextP2.text= ScoreP2.ToString() + " : " + ScoreP1.ToString();
		}

		else if(a==2 || a==3)
		{
			ScoreP2 +=1;
			ScoreTextP1.text= ScoreP1.ToString() + " : " + ScoreP2.ToString();
			ScoreTextP2.text= ScoreP2.ToString() + " : " + ScoreP1.ToString();
		}
	}
		
	//Checks if we have available lives and either end the game or continue
	void checkLives()
	{
		if(AvailableLives<0)
		{
			//Disable the buttons to prevent further user input
			Buttons.interactable=false;

			//Set gameOver bool to true
			GameOver=true;

			//Check highscore
			if(score>highscore)
				PlayerPrefs.SetFloat("Highscore", score);

			//Disable game canvas
			GameCanvas.SetActive(false);

			//Show gameOver canvas
			GameOverCanvas.SetActive(true);

			//Display score for the round
			FinalScore.text=score.ToString("0");

			//Display highscore
			CurHighscore.text="Highscore: " + PlayerPrefs.GetFloat("Highscore",0).ToString("0");
		}
	}

	void CheckWinner()
	{
		if(ScoreP1 ==WinningScore || ScoreP2 ==WinningScore)
		{
			//Disable the buttons to prevent further user input
			ButtonsPlayer1.interactable=false;
			ButtonsPlayer2.interactable=false;

			//Set gameOver bool to true
			GameOver=true;

			//Disable game canvas
			H2HCanvas.SetActive(false);
			//Show gameOver canvas
			GameOverCanvas.SetActive(true);

			//Display Texts
			if(ScoreP1>ScoreP2)
			{
				FinalScore.text="Player 1";
				CurHighscore.text="WINS";
			}
			else if(ScoreP2>ScoreP1)
			{
				FinalScore.text="Player 2";
				CurHighscore.text="WINS";
			}
		}
	}

	//Diplays the next question is single player mode
	void NextQuestion()
	{
		//Reset Timer
		TimeLeft=7;

		//Reset fill amount
		TimerFill.fillAmount=1;

		//Reset timer text
		Timer.text=TimeLeft.ToString("0");

		//Reset timer fill color
		TimerFill.color=ColorList[1];

		//Hide the result display text
		ResultDisplay.gameObject.SetActive(false);

		//Set "Answered" bool to false
		Answered=false;

		//Displays the next question on the list
		QuestionDisplay.text=Questions[currentQuiz].question;

		//Re-enable the buttons to allow user input
		Buttons.interactable=true;

		if(TimeOver == true)
		{
			StartCoroutine(UpdateTimer());
			TimeOver = false;
		}

		if(SoundFX.isPlaying)
			SoundFX.Stop();
	}

	//Diplays the next question H2H mode
	void NextH2HQuestion()
	{
		//Displays the next question on the list
		for (int q=0;q<QuestionDisplays.Length;q++)
			QuestionDisplays[q].text=Questions[currentH2HQuiz].question;

		//Re-enable the buttons to allow user input
		ButtonsPlayer1.interactable=true;
		ButtonsPlayer2.interactable=true;

		if(SoundFX.isPlaying)
		{
			SoundFX.Stop();
		}
	}


	//Countdown the time left to answer a question
	IEnumerator UpdateTimer()
	{		
		if ( TimeLeft > 0 && Answered == false  && GameOver == false)
		{
			yield return new WaitForSeconds(1);
			if(Answered)
			{
				TimeOver=true;
				ticking = false;
				yield break;
			}
			
			TimeLeft -= 1;
			Timer.text=TimeLeft.ToString("0");

			//Change the fill color based on time left
			if(TimerFill.fillAmount>0.7)
				TimerFill.color=ColorList[1];
			else if(TimerFill.fillAmount>0.45)
				TimerFill.color=ColorList[3];
			else if(TimeLeft<=5)
			{
				TimerFill.color=ColorList[0];
				if(ticking == false && Answered == false) SoundFX.PlayOneShot(SoundEffect[2]);
				ticking = true;
			}

			//Animate the timer graphic using fill amount
			TimerFill.fillAmount = TimeLeft/15;

			StartCoroutine(UpdateTimer());

		}
		else if (Answered == false && GameOver == false)
		{
			//Disable the buttons to prevent further user input
			Buttons.interactable=false;

			//Active the result display text
			ResultDisplay.gameObject.SetActive(true);

			//Set the result display text color
			ResultDisplay.GetComponent<Text>().color=ColorList[0];

			//Display Time's Up text
			ResultDisplay.text="TIME'S UP!";

			//Stop any audioclip playing
			SoundFX.Stop();

			//Play Sound Effect
			SoundFX.PlayOneShot(SoundEffect[1]);

			//Update remaining lives
			if(AvailableLives>=0) Lives[AvailableLives].color=ColorList[2];

			//Remove one life
			if(AvailableLives>=0) AvailableLives -=1;

			//Check if we have available lives remaining
			Invoke("checkLives", 0.75f);

			//Update the correct answer counter
			UpdateCounter();

			//Increment the question counter if we have more questions
			if(currentQuiz<Questions.Count-1) currentQuiz +=1;

			//Display the next question in x seconds
			Invoke("NextQuestion", 1.5f);

			TimeOver = true;
			ticking = false;
		}
		else if (Answered == true)
		{
			TimeOver = true;
			ticking = false;
		}
	}

	//Call this to pause the game
	public void PauseGame()
	{
		//Freeze gameplay
		Time.timeScale=0;

		//Pause audiosource
		if(ticking) SoundFX.Pause();

		//Set "Paused" bool to true
		Paused=true;

		//Show Pause Canvas
		PauseCanvas.SetActive(true);
	}

	//Call this to un-pause the game
	public void ResumeGame()
	{
		//Un-freeze gameplay
		Time.timeScale=1;

		//Un-Pause audiosource
		if(ticking)
		{
			SoundFX.Stop();
			SoundFX.PlayOneShot(SoundEffect[2]);
		}

		//Set "Paused" bool to false
		Paused=false;

		//Hide Pause Canvas
		PauseCanvas.SetActive(false);
	}

	//Call this to quit the game and return to main menu
	public void ForfeitGame()
	{		
		//Set the "Paused" bool to false
		Paused=false;

		//Set the "Started" bool to false
		Started=false;

		//Disable gameover Canvas
		if(GameCanvas) GameCanvas.SetActive(false);

		//Disable H2H Canvas
		if(H2HCanvas) H2HCanvas.SetActive(false);

		//Disable category Canvas
		if(CategoryCanvas) CategoryCanvas.SetActive(false);

		//Disable gameover Canvas
		if(GameOverCanvas) GameOverCanvas.SetActive(false);

		//Disable pause Canvas
		if(PauseCanvas) PauseCanvas.SetActive(false);

		//Enable menu Canvas
		if(MenuCanvas) MenuCanvas.SetActive(true);

		//Set GameOver to true
		GameOver=true;

		//Set timescale to 1
		Time.timeScale=1;

	}

	//Call this to retry/restart the game
	public void Retry()
	{
		if(!H2HMode)
		{
			//Initialize new game
			PrepareNewGame();

			//Set the "Started" bool to true
			Started=true;

			//Set gameOver bool to false
			GameOver=false;

			//Displays the first question on the list
			NextQuestion();

			//Start countdown timer
			StartCoroutine(UpdateTimer());
		}
		else
		{
			//Initialize new game
			PrepareNewH2HGame();

			//Set the "Started" bool to true
			Started=true;

			//Set gameOver bool to false
			GameOver=false;

			//Displays the first question on the list
			NextH2HQuestion();
		}
	}

	//Set up a new game
	void PrepareNewGame()
	{
		//Set timescale to 1
		Time.timeScale=1;

		//Reset the lives counter
		AvailableLives=4;

		//Update the number of lives available
		for(int i=0; i<Lives.Length;i++)
		{
			Lives[i].color=ColorList[0];
		}

		//Re-shuffles the question list
		ShuffleQuestions();

		//Reset the current question counter
		currentQuiz =0;

		//Reset the correct answer counter
		CorrectAnswer = 0;

		//Set the quiz counter text to nil
		QuizCounter.text= "0 / 0";

		//Reset the score counters
		score =0;
		scoreCount =0;

		//Set the score counter text to nil
		ScoreText.text =Prefix + "0";

		//Get current highscore
		highscore = PlayerPrefs.GetFloat ("Highscore",0);

		//Disable menu Canvas
		if(MenuCanvas) MenuCanvas.SetActive(false);

		//Disable menu Canvas
		if(CategoryCanvas) CategoryCanvas.SetActive(false);

		//Disable gameover Canvas
		if(GameOverCanvas) GameOverCanvas.SetActive(false);

		//Disable pause Canvas
		if(PauseCanvas) PauseCanvas.SetActive(false);

		//Disable H2H Canvas
		if(H2HCanvas) H2HCanvas.SetActive(false);

		//Enable Game Canvas
		GameCanvas.SetActive(true);

	}

	//Set up a new H2H game
	void PrepareNewH2HGame()
	{
		//Set timescale to 1
		Time.timeScale=1;

		//Re-shuffles the question list
		ShuffleQuestions();

		//Reset the current question counter
		currentH2HQuiz =0;

		//Reset the correct answer counter
		ScoreP1 = 0;
		ScoreP2 = 0;

		//Set the score counters text to nil
		ScoreTextP1.text= "0 : 0";
		ScoreTextP2.text= "0 : 0";

		//Disable menu Canvas
		if(MenuCanvas) MenuCanvas.SetActive(false);

		//Disable category Canvas
		if(CategoryCanvas) CategoryCanvas.SetActive(false);

		//Disable gameover Canvas
		if(GameOverCanvas) GameOverCanvas.SetActive(false);

		//Disable pause Canvas
		if(PauseCanvas) PauseCanvas.SetActive(false);

		//Disable Game Canvas
		GameCanvas.SetActive(false);

		//Enable H2H Canvas
		if(H2HCanvas) H2HCanvas.SetActive(true);

	}

	//Call this to play a specific sound when a button is pressed
	public void PlaySound(int audio)
	{
		//Stop any audio playing and play new effect
		SoundFX.Stop();
		SoundFX.PlayOneShot(SoundEffect[audio]);
	}

	//Call this to redirect the user to a link
	public void URL(string link)
	{
		Application.OpenURL(link);
	}
}