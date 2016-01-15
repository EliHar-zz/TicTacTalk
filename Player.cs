using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechToTextWPFSample
{
    class Player
    {
        private string name; // player's name
        private char token; // the player's token or sign

        // Default constructor
        public Player()
        {
            this.name = null;
        }

        // Parameterized constructor
        public Player(String name, char token)
        {
            this.name = name;
            this.token = token;
        }

        // Getter for name
        public String getName()
        {
            return name;
        }

        // setter for name
        public void setName(String name)
        {
            this.name = name;
        }

        // retter for the token of the player
        public char gettoken()
        {
            return this.token;
        }

        // Method to mark the board at a given spot. Takes a board and spot row and column to mark.
        // The method returns true if the spot was available, false otherwise
        public bool mark(Game g, string location)
        {
            String A = "abc"; // Max size of board is 9, so the max amount of letters is up to 'I'
            int rowNum = A.IndexOf(location[0]);

            if (g.getBoard()[rowNum, int.Parse(location[1]+"") - 1] == 0)
            {
                g.getBoard()[rowNum, int.Parse(location[1] + "") - 1] = token;

                return true;
            }
            
            else
            {
                return false;
            }
        }
    }
}
