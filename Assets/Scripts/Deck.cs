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

    public Text banca;
    private int valorBanca = 1000;
    public Text apuesta;
    private int valorApuesta;
    public Button apuestaMas;
    public Button apuestaMenos;

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
            else if (valorAuxiliar >= 10) values[i] = 10;
            
            else values[i] = valorAuxiliar;
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
            if (values[1] + values[3] == 21) JuegoTerminado(1);

            //En caso contrario, ha sido solo el jugador el que ha obtenido Blackjack
            else JuegoTerminado(2);
        }
        CalculateProbabilities();
    }

    private void CalculateProbabilities()
    {
        probMessage.text = PrimeraProbabilidad() + "% | " +
            SegundaProbabilidad().ToString() + "% | " +
            TerceraProbabilidad()+"%";
    }

    /* *********************** Primera Probabilidad ********************************
    * **************************************************************************** *
    * Probabilidad de que el dealer tenga más puntuación que el jugador teniendo * *
    * *********************************************************una carta oculta. * *
    ****************************************************************************** */
    private float PrimeraProbabilidad()
    {
        int playerPoints = values[0] + values[2];
        int dealerPoints = values[3];

        float cartasFavorables = 0;

        //Si la carta oculta del dealer puede favorecerle
        //se cuenta como una carta favorable
        if ( (dealerPoints + values[1]) > playerPoints) cartasFavorables++;

        //Si la carta oculta del dealer es un As y puede favorecele actuando como un 1
        //se cuenta como una carta favorable
        if (values[1] == 11 && (dealerPoints + 1) > playerPoints) cartasFavorables++;

        //Se realiza un recorrido por todos los valores de las cartas que están por salir y se
        //cuentas las cartas favorables para que el valor del dealer sea mayor que el valor del jugador
        for (int i = cardIndex; i < values.Length - 1; i++)
        {
            if ( (dealerPoints + values[i]) > playerPoints) cartasFavorables++;

            //Si el valor del dealer es superior a 10, los ases contarán 1 en vez de 11 y se
            //añadirán a cartas favorables si lo son.
            if (dealerPoints > 10 && (dealerPoints + values[i]) > playerPoints) cartasFavorables++;
        }
        return Mathf.Floor( cartasFavorables / (52 - cardIndex)  *100 );
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
        return Mathf.Floor(cartasFavorables / (52 - cardIndex) * 100);
    }

    /* *********************** Tercera Probabilidad ******************************* *
     * **************************************************************************** *
     * Probabilidad de que el jugador obtenga más de 21 si pide una carta. ******** *
     * **************************************************************************** *
     ****************************************************************************** */
    private float TerceraProbabilidad()
    {
        int playerPoints = values[0] + values[2];
        int dealerPoints = values[3];

        float cartasFavorables = 0;

        //Si la carta oculta del dealer puede favorecer la probabilidad
        //se cuenta como una carta favorable, excepto si es un As
        if ( values[1] != 11 && (playerPoints + values[1]) > 21) cartasFavorables++;

        //Si la carta oculta del dealer es un As se cuenta como un 1 en caso de que
        //la puntuación del jugador sea mayor de 10
        if ( values[1] == 11 && playerPoints > 10 && (playerPoints + 1 > 21) ) cartasFavorables++;

        //Si la carta oculta del dealer es un As se cuenta como un 11 en caso de que
        //la puntuación del jugador sea menor de 11
        if (values[1] == 11 && playerPoints < 11) cartasFavorables++;

        //Se realiza un recorrido por todos los valores de las cartas que están por salir
        for (int i = cardIndex; i < values.Length; i++)
        {
            //Se cuentan las cartas favorables para que el jugador obtenga más de 21
            //exceptuando los ases
            if ( values[i] != 11 && (playerPoints + values[i]) > 21) cartasFavorables++;

            //Si el valor del jugador es superior a 10, los ases contarán como 1 y se
            //añadirán a cartas favorables.
            if (playerPoints > 10 && values[i] == 11 && (playerPoints + 1) > 21) cartasFavorables++;

            //Si el valor del jugador es inferior a 11, los ases contarán como 11 y se
            //añadirán a cartas favorables.
            if (playerPoints < 11 && values[i] == 11 && (playerPoints + values[i]) > 21) cartasFavorables++;
        }

        //Si el jugador consigue un blackjack, no existirán cartas favorables
        if (playerPoints == 21) cartasFavorables = 0;

        return Mathf.Floor(cartasFavorables / (52 - cardIndex) * 100);
    }

    void PushDealer()
    {
        dealer.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
    }

    void PushPlayer()
    {
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
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
            JuegoTerminado(0);
        }

        if(playerPoints == 21)
        {
            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
            if (dealerPoints == 21) JuegoTerminado(1);
            else JuegoTerminado(2);
        }
        CalculateProbabilities();
    }

    public void Stand()
    {
        //Se voltea la primera carta del dealer
        dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);

        //Se ocultan los botones
        playAgainButton.interactable = false;
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
            if (playerPoints > dealerPoints || dealerPoints > 21) JuegoTerminado(2);
            else if (playerPoints < dealerPoints) JuegoTerminado(0);
            else if (playerPoints == dealerPoints) JuegoTerminado(1);
        }
    }

    public void PlayAgain()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;

        apuestaMas.interactable = false;
        apuestaMenos.interactable = false;
        playAgainButton.interactable = false;

        finalMessage.text = "";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();
        cardIndex = 0;
        ShuffleCards();
        StartGame();
    }

    public void SubirApuesta()
    {
        if (valorApuesta < valorBanca)
        {
            valorApuesta += 10;
            apuesta.text = valorApuesta.ToString() + "€";
            playAgainButton.interactable = true;
        }
    }
    
    public void BajarApuesta()
    {
        if (valorApuesta > 0)
        {
            valorApuesta -= 10;
            apuesta.text = valorApuesta.ToString() + "€";
            if (valorApuesta == 0) playAgainButton.interactable = false;
        }
    }

    private void JuegoTerminado(int codigoJuego)
    {
        playAgainButton.interactable = false;
        hitButton.interactable = false;
        stickButton.interactable = false;

        apuestaMas.interactable = true;
        apuestaMenos.interactable = true;

        apuesta.text = "0€";
        

        if(codigoJuego == 0)
        {
            finalMessage.text = "¡HAS PERDIDO!";
            valorBanca -= valorApuesta;
        }

        if(codigoJuego == 1) finalMessage.text = "¡EMPATE!";

        if (codigoJuego == 2)
        {
            finalMessage.text = "¡HAS GANADO!";
            valorBanca += (2 * valorApuesta);
        }

        if(codigoJuego == 3)
        {
            finalMessage.text = "¡BLACKJACK!";
            valorBanca += (2 * valorApuesta);
        }
        banca.text = valorBanca.ToString() + "€";
        valorApuesta = 0;
    }
}
