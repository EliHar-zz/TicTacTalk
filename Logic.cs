using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpeechToTextWPFSample
{
    class Logic
    {
        public static String message = null;
        public static char currentToken;
        public void DoWork()
        {

            //*********************** LOGIC STARTS

            const int numPlayers = 2; // default number of players is 2

            // print a welcome message
            Console.Write("\t\t\t======================\n\n");
            Console.Write("\t\t\tWelcome to Tic-Tac-Toe\n\n");
            Console.Write("\t\t\t======================\n\n");

            int size; // declare a variable to hold the board size
            Game g; // declare a game object
            Player[] players; // declare array of players
            bool winner; // declare a bool variable to keep indicate if there is a winner or not

            // to start the game, assume the user answer is YES (ie. 'Y')
            // This loop corresponds to the whole program running
            String answer = "Y";
            while (answer == "Y")
            {
                size = 3;

                // create a game with the size given by the user
                g = new Game(size);

                // initialize and array of players using the default number of players
                players = new Player[numPlayers];

                // a bool variable to keep indicate if there is a winner or not
                winner = false;
                char[] tokens = { 'X', 'O' };
                string[] names = { "Elias", "Chris" };
                for (int i = 0; i < players.Length; i++)
                {
                    Console.Write("Enter player " + (i + 1) + " name: ");
                    //TTSSample.Program.sayThis("Enter player " + (i + 1) + " name");
                    
                    String name = names[i];

                    Console.Write("Enter player " + (i + 1) + " token: ");
                    char token = tokens[i];
                    players[i] = new Player(name, token);

                    Console.Write("\n");
                }

                Console.Write("\n\n\t\t\tLet the game start...\n\n");
                TTSSample.Program.sayThis("Let the battle commence!");
                // Loop the game as long as the board is not full, and there is no winner yet
                // This loop corresponds to one game
                while (!g.isFull() && !winner)
                {
                    
                    // loop through the array of players to give turns
                    for (int i = 0; i < players.Length; i++)
                    {
                        currentToken = players[i].gettoken();
                        if (g.isFull())
                            break;
                        message = null;
                        Console.WriteLine(g);

                        Console.Write("\nEnter spot to mark: ");
                        TTSSample.Program.sayThis(players[i].getName()+"\'s turn");
                        //listen to tha talk
                        while (message==null)
                        {

                        }

                        string location = ""; // ********** entry via mic

                        Console.Write("\n");



                        bool isMarked = players[i].mark(g, message);

                        // While the spot is not marked due to a pre-existing token
                        while (!isMarked)
                        {
                            Console.Write("\nEnter spot to mark: ");

                            location = ""; // ********** entry via mic

                            Console.Write("\n");

                            isMarked = players[i].mark(g, "A1");

                            Console.Write("\n");
                        }

                        // Once the player has marked a spot, check if they won
                        if (g.didWin(players[i].gettoken()))
                        {
                            Console.WriteLine(g);
                            Console.WriteLine(players[i].getName() + " is the winner!");

                            //call pop-up from here
                            if (Application.Current.Dispatcher.CheckAccess())
                            {
                                MessageBox.Show("Congratulations " + players[i].getName() + " !!", "AWWWWW YEAHHHHHH");
                            }
                            else {
                                Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => {
                                    MessageBox.Show("Congratulations " + players[i].getName() + " !!", "AWWWWW YEAHHHHHH");
                                }));
                            }

                            TTSSample.Program.sayThis(players[i].getName() + " is the winner!");
                            winner = true;
                            answer = "n";
                            break;
                        }
                    }
                }
                if (g.isFull() && !winner)
                {
                    Console.Write("It's a draw! Intense competition\n\n");
                }
                Console.Write("Would you like to play again (Y/N)?");

                winner = false;
            }
            Console.Write("\n\nHope to see you again...");

            //*********************** LOGIC ENDS
        }

        public static void setMessage(String s)
        {
            message = s;
        }
    }
}
