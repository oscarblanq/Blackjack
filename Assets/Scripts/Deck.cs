using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    public Sprite[] faces;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Text finalMessage;
    public Text probMessage;

    public int[] values = new int[52];
    int cardIndex = 0;    
       
    private void Awake()
    {    
        InitCardValues();

    }

    private void Start()
    {
        ShuffleCards();
        StartGame();        
    }

    private void InitCardValues()
    {
        int valorAuxiliar = 1;
        for (int i = 0; i < values.Length; i++)
        {
            valorAuxiliar++;

            if (i == 0 || i == 13 || i == 26 || i == 39)
            {
                values[i] = 11;
                valorAuxiliar = 1;
            }

            else if (valorAuxiliar >= 10)
            {
                values[i] = 10;
            }
            
            else
            {
                values[i] = valorAuxiliar;
            }
        }
    }

    private void ShuffleCards()
    {
        for (int i = 0; i < values.Length; i++)
        {
            int cartaAzar = Random.Range(i, values.Length);

            //Barajamos los sprites
            Sprite cartaActual = faces[i];
            faces[i] = faces[cartaAzar];
            faces[cartaAzar] = cartaActual;

            //Barajamos los valores de las cartas 
            int valorActual = values[i];
            values[i] = values[cartaAzar];
            values[cartaAzar] = valorActual;
        }
    }

    void StartGame()
    {        
        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();
        }
        
        //Si el jugador obtiene 21 puntos
        if (values[0] + values[2] == 21)
        {
            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
            hitButton.interactable = false;
            stickButton.interactable = false;

            //Si el dealer ha obtenido 21 es empate.
            if (values[1] + values[3] == 21) finalMessage.text = "EMPATE";

            //En caso contrario, ha sido solo el jugador el que ha obtenido Blackjack
            else finalMessage.text = "¡BLACKJACK!";
        }
        CalculateProbabilities();
    }

    private void CalculateProbabilities()
    {
        /*TODO:
         * Calcular las probabilidades de:
         * - Teniendo la carta oculta, probabilidad de que el dealer tenga más puntuación que el jugador
         * - Probabilidad de que el jugador obtenga más de 21 si pide una carta          
         */

        probMessage.text = SegundaProbabilidad().ToString() + "%";
    }

    /* *********************** Segunda Probabilidad ******************************* *
     * **************************************************************************** *
     * Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta *
     * **************************************************************************** *
     ****************************************************************************** */
    private float SegundaProbabilidad()
    {
        int playerPoints = values[0] + values[2];
        int dealerPoints = values[3];
        
        float cartasFavorables = 0;

        //Si la carta oculta del dealer puede favorecer al jugador
        //se cuenta como una carta favorable
        if (17 <= playerPoints + values[1] && playerPoints + values[1] <= 21) cartasFavorables++;

        //Si la carta oculta del dealer es un As y puede favorecer al jugador actuando como un 1
        //se cuenta como una carta favorable
        if (values[1] == 11 && 17 <= (playerPoints + 1) && (playerPoints + 1) <= 21) cartasFavorables++;

        //Se realiza un recorrido por todos los valores de las cartas que están por salir
        //y se cuentan las cartas favorables para que el valor del jugador esté comprendido entre 17 y 21
        for (int i = cardIndex; i < values.Length; i++)
        {
            if (17 <= playerPoints + values[i] && playerPoints + values[i] <= 21) cartasFavorables++;

            //Si el valor del jugador es superior a 10, los ases contarán 1 en vez de 11 y se
            //añadirán a cartas favorables si lo son.
            if (playerPoints > 10 && 17 <= (playerPoints + 1) && (playerPoints + 1) <= 21 && values[i] == 11)
            {
                cartasFavorables++;
            }
        }
        return Mathf.Floor((cartasFavorables / (52 - cardIndex)) * 100);
    }

    void PushDealer()
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
         */
        dealer.GetComponent<CardHand>().Push(faces[cardIndex],values[cardIndex]);
        cardIndex++;        
    }

    void PushPlayer()
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
         */
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]/*,cardCopy*/);
        cardIndex++;
        CalculateProbabilities();
    }

    public void Hit()
    {
        //Repartimos carta al jugador
        PushPlayer();

        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points;

        if(playerPoints > 21)
        {
            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
            finalMessage.text = "¡HAS PERDIDO!";
            hitButton.interactable = false;
            stickButton.interactable = false;
        }

        if(playerPoints == 21)
        {
            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
            if (dealerPoints == 21) finalMessage.text = "EMPATE";
            else finalMessage.text = "¡HAS GANADO!";
            hitButton.interactable = false;
            stickButton.interactable = false;
        }
        CalculateProbabilities();
    }

    public void Stand()
    {
        //Se voltea la primera carta del dealer
        dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);

        //Se ocultan los botones
        hitButton.interactable = false;
        stickButton.interactable = false;

        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points;

        while (dealerPoints < 17)
        {
            PushDealer();
            dealerPoints = dealer.GetComponent<CardHand>().points;
        }

        if(dealerPoints >= 17)
        {

            if (playerPoints > dealerPoints || dealerPoints > 21) finalMessage.text = "¡HAS GANADO!";
            else if (playerPoints < dealerPoints) finalMessage.text = "¡HAS PERDIDO!";
            else if (playerPoints == dealerPoints) finalMessage.text = "EMPATE";
        }
    }

    public void PlayAgain()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        finalMessage.text = "";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();          
        cardIndex = 0;
        ShuffleCards();
        StartGame();
    }
}
