using ludo_game.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;
namespace ludo_game
{
    public partial class Form1 : Form
    {
        private PictureBox board = new PictureBox();
        private PictureBox dice = new PictureBox();
        private Label whosTurn = new Label();
        private List<List<Pawn>> allPawns = new List<List<Pawn>>();
        private List<string> turn = new List<string> { "blue", "yellow", "green", "red" };
        private List<Label> places = new List<Label>();
        private Random random = new Random();
        private int indexOfTurn = 0;
        private int diceCount = 1;
        private int place = 0;
        private bool movePending = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeBoard();
            InitializeDice();
            InitializeWhosTurnLabel();
            InitializePawns();
            InitializePlaces();
            RunGameLoop();
        }
        public void InitializePlaces()
        {
            int startY = 350;  
            int spacing = 10; 

            for (int i = 0; i < 4; i++)
            {
                Label placeLabel = new Label();
                placeLabel.Size = new Size(200, 30); 
                placeLabel.Location = new Point(550, startY + (spacing + 30) * i);  
                places.Add(placeLabel);
                this.Controls.Add(placeLabel);
            }
        }



        private void InitializeBoard()
        {
            board.Image = Resources.board;
            board.SizeMode = PictureBoxSizeMode.StretchImage;
            board.Size = new Size(500, 500);
            board.Location = new Point(0, 0);
            this.Controls.Add(board);
        }

        private void InitializeDice()
        {
            dice.Size = new Size(150, 150);
            dice.Location = new Point(550, 200);
            this.Controls.Add(dice);
            diceCount = random.Next(1, 7);
        }

        private void InitializeWhosTurnLabel()
        {
            whosTurn.Size = new Size(500, 250);
            whosTurn.Location = new Point(550, 10);
            this.Controls.Add(whosTurn);
        }

        private void InitializePawns()
        {
            allPawns.Add(LoadPawns("blue", 72, 130, 360, 415));
            allPawns.Add(LoadPawns("yellow", 418, 360, 360, 415));
            allPawns.Add(LoadPawns("green", 418, 360, 70, 125));
            allPawns.Add(LoadPawns("red", 72, 130, 70, 125));

            foreach (var pawns in allPawns)
            {
                foreach (Pawn pawn in pawns)
                {
                    board.Controls.Add(pawn);
                    pawn.Click += Pawn_Click;
                    pawn.BringToFront();
                }
            }
        }

        private List<Pawn> LoadPawns(string color, int x1, int x2, int y1, int y2)
        {
            List<Pawn> pawns = new List<Pawn>();
            string resourceName = color + "_pawn";
            Image image = (Image)Resources.ResourceManager.GetObject(resourceName);

            if (image == null)
            {
                throw new Exception($"Image for {resourceName} is null.");
            }

            for (int i = 0; i < 4; i++)
            {
                Point location;
                switch (i)
                {
                    case 0:
                        location = new Point(x1, y1);
                        break;
                    case 1:
                        location = new Point(x2, y1);
                        break;
                    case 2:
                        location = new Point(x1, y2);
                        break;
                    case 3:
                        location = new Point(x2, y2);
                        break;
                    default:
                        throw new Exception("Invalid pawn index");
                }
                Pawn pawn = new Pawn(image, color, location);
                pawns.Add(pawn);
            }

            return pawns;
        }

        private void Pawn_Click(object sender, EventArgs e)
        {
            Pawn selectedPawn = (Pawn)sender;

            if (selectedPawn.ColorPlayer != turn[indexOfTurn])
            {
                MessageBox.Show("It's not your turn!");
                return;
            }

            Point newLocation = selectedPawn.Location;

            if(!selectedPawn.IsActive && diceCount == 6 && selectedPawn.HowManyToWin != 0)
            {
                // If the roll is 6 and the pawn is not active and has not won, place it in the starting position
                switch (turn[indexOfTurn])
                {
                    case "blue":
                        newLocation = new Point(215, 436);
                        selectedPawn.LastChangeDirectionPoint = new Point(215, 468);
                        selectedPawn.SafePosition = new Point(87, 276);
                        selectedPawn.BeginMeta = new Point(247, 468);
                        break;
                    case "yellow":
                        newLocation = new Point(439, 276);
                        selectedPawn.LastChangeDirectionPoint = new Point(471, 276);
                        selectedPawn.SafePosition = new Point(279, 404);
                        selectedPawn.BeginMeta = new Point(471, 244);
                        break;
                    case "green":
                        newLocation = new Point(279, 52);
                        selectedPawn.LastChangeDirectionPoint = new Point(279, 20);
                        selectedPawn.SafePosition = new Point(407, 212);
                        selectedPawn.BeginMeta = new Point(247, 20);
                        break;
                    case "red":
                        newLocation = new Point(55, 212);
                        selectedPawn.LastChangeDirectionPoint = new Point(23, 212);
                        selectedPawn.SafePosition = new Point(215, 84);
                        selectedPawn.BeginMeta = new Point(23, 244);
                        break;
                }
                selectedPawn.IsActive = true;
                selectedPawn.Location = newLocation;
                selectedPawn.OriginalField = selectedPawn.Location;

            }
            else if(selectedPawn.IsActive && selectedPawn.HowManyToWin >= diceCount)
            {
                //Code for standard moves on the board
                ReturnToOriginalField(selectedPawn);
                newLocation = Move(selectedPawn.Location, selectedPawn);
                selectedPawn.Location = newLocation;
            }
            else
            {
                //Code when it cannot be moved
                MessageBox.Show("You can't move this pawn now!");
                return;
            }
            CollisionAndCapturing(selectedPawn);
            MoreThanOne();
            movePending = false;
        }
        private async void RunGameLoop()
        {
            while (place < 3)
            {
                await RollDiceAndCheckConditions();
            }
            string result = " | ";
            foreach (Label p in places)
            {
                result += p.Text;
                result += " | ";
            }
            MessageBox.Show(result);
            return;
        }

        private async Task RollDiceAndCheckConditions()
        {
            diceCount = random.Next(1, 7);
            this.Invoke((MethodInvoker)delegate
            {
                string resourceName = "_" + diceCount.ToString();
                Image image = (Image)Resources.ResourceManager.GetObject(resourceName);
                if (image == null)
                {
                    throw new Exception($"Image for {resourceName} is null.");
                }
                dice.Image = image;
                whosTurn.Text = "Now is turn: " + turn[indexOfTurn];
            });

            bool condition1 = allPawns[indexOfTurn].All(pawn => !pawn.IsActive && pawn.Location == pawn.InitialPosition) && diceCount != 6;
            bool condition2 = allPawns[indexOfTurn].All(pawn => !pawn.IsActive && pawn.HowManyToWin == 0);
            bool condition3 = allPawns[indexOfTurn].Any(pawn => pawn.IsActive) && allPawns[indexOfTurn].Where(pawn => pawn.IsActive).All(pawn => pawn.HowManyToWin < diceCount);

            int i = 0;
            //If all pieces are at the finish or in the initialize position
            foreach (var pawn in allPawns[indexOfTurn])
            {
                if (!pawn.IsActive && pawn.Location == pawn.InitialPosition && diceCount != 6) i++;
                else if (!pawn.IsActive && pawn.HowManyToWin == 0) i++;
            }
            
            if (condition1 || condition2 || condition3 || i == 4)
            {
                await Task.Delay(2000);
                AdvanceTurn();
            }
            else
            {
                movePending = true;
                while (movePending) // Wait until move is made
                {
                    await Task.Delay(100);
                }
                AdvanceTurn();
            }
        }

        private void AdvanceTurn()
        {
            indexOfTurn = (indexOfTurn + 1) % turn.Count;
            diceCount = random.Next(1, 7);
        }
    

    private Point Move(Point currentPoint, Pawn selectedPawn)
        {
            int steps = diceCount;

            if (selectedPawn.IsChanged == true)
            {
                selectedPawn.Location = selectedPawn.OriginalField;
                selectedPawn.IsChanged = false;
            }
            currentPoint = selectedPawn.OriginalField;
            List<Point> pointsChangeDirection = new List<Point>
            {
                new Point(215, 468), // A <- changes direction to up
                new Point(23, 276), // B <- changes direction to up
                new Point(183, 212), // C <- changes direction to up (diagonally)
                new Point(279, 20),  // D <- changes direction to down
                new Point(471, 212), // E <- changes direction to down
                new Point(311, 276), // F <- changes direction to down (diagonally)
                new Point(215, 308), // G <- changes direction to left (diagonally)
                new Point(471, 276), // H <- changes direction to left
                new Point(279, 468), // I <- changes direction to left
                new Point(23, 212),  // J <- changes direction to right
                new Point(215, 20),  // K <- changes direction to right
                new Point(279, 180)  // L <- changes direction to right (diagonally)
            };
            

            while (steps > 0)
            {
                int indexOfLastDirectionPoint = pointsChangeDirection.FindIndex(p => p == selectedPawn.LastChangeDirectionPoint);

                if (selectedPawn.HowManyToWin < 6 || currentPoint == selectedPawn.BeginMeta)
                {
                    selectedPawn.WasAtBeginMeta = true;
                    return Winning(selectedPawn, steps, currentPoint);
                }

                // Ruch w górę
                if (indexOfLastDirectionPoint >= 0 && indexOfLastDirectionPoint <= 2)
                {
                    if (currentPoint == pointsChangeDirection[2])
                    {

                        selectedPawn.LastChangeDirectionPoint = currentPoint;
                        currentPoint = MoveRight(currentPoint);
                    }
                    currentPoint = MoveUp(currentPoint);
                }
                // Ruch w dół
                else if (indexOfLastDirectionPoint >= 3 && indexOfLastDirectionPoint <= 5)
                {
                    if (currentPoint == pointsChangeDirection[5])
                    {

                        selectedPawn.LastChangeDirectionPoint = currentPoint;
                        currentPoint = MoveLeft(currentPoint);
                    }
                    currentPoint = MoveDown(currentPoint);
                }
                // Ruch w lewo
                else if (indexOfLastDirectionPoint >= 6 && indexOfLastDirectionPoint <= 8)
                {
                    if (currentPoint == pointsChangeDirection[6])
                    {

                        selectedPawn.LastChangeDirectionPoint = currentPoint;
                        currentPoint = MoveUp(currentPoint);
                    }
                    currentPoint = MoveLeft(currentPoint);

                }
                // Ruch w prawo
                else if (indexOfLastDirectionPoint >= 9 && indexOfLastDirectionPoint <= 11)
                {
                    if (currentPoint == pointsChangeDirection[11])
                    {

                        selectedPawn.LastChangeDirectionPoint = currentPoint;
                        currentPoint = MoveDown(currentPoint); 
                    }
                    currentPoint = MoveRight(currentPoint);
                }

                // Update lastChangeDirectionPoint if the pawn is on a change of direction point
                if (pointsChangeDirection.Any(p => p == currentPoint))
                {
                    selectedPawn.LastChangeDirectionPoint = currentPoint;
                }

                steps--;
            }
            selectedPawn.Location = currentPoint;
            
            if (selectedPawn.IsChanged == false)
            {
                selectedPawn.OriginalField = selectedPawn.Location;
            }


            return currentPoint;
        }

        private Point MoveUp(Point currentPoint)
        {
            currentPoint.Y -= 32;
            return currentPoint;
        }
        private Point MoveDown(Point currentPoint)
        {
            currentPoint.Y += 32;
            return currentPoint;
        }
        private Point MoveLeft(Point currentPoint)
        {
            currentPoint.X -= 32;
            return currentPoint;
        }
        private Point MoveRight(Point currentPoint)
        {
            currentPoint.X += 32;
            return currentPoint;
        }

        private void CapturingPawn(Pawn selectedPawn, Pawn pawnToCapture)
        {

            if (pawnToCapture.Location == pawnToCapture.BeginMeta)
            {
                pawnToCapture.WasAtBeginMeta = false;
            }
            pawnToCapture.Location = pawnToCapture.InitialPosition;
            pawnToCapture.OriginalField = pawnToCapture.Location;
            pawnToCapture.IsActive = false;
            MessageBox.Show("Captured " + pawnToCapture.ColorPlayer + " pawn!");

        }

        private void MoreThanOne()
        {
            // Find all labels to remove if the number of pieces on the field has dropped to 1 or 0
            var labelsToRemove = this.Controls.OfType<Label>()
                .Where(l => l.Name.StartsWith("countLabel_"))
                .Where(l =>
                {
                    var coords = l.Name.Split('_').Skip(1).Select(int.Parse).ToArray();
                    Point labelPoint = new Point(coords[0], coords[1]);
                    return allPawns.SelectMany(p => p).Count(p => p.Location == labelPoint) <= 1;
                })
                .ToList();

            // Remove labels
            foreach (var label in labelsToRemove)
            {
                this.Controls.Remove(label);
                label.Dispose();
            }

            // Iterate through all pawns and update labels
            foreach (var pawns in allPawns)
            {
                foreach (var pawn in pawns)
                {
                    var groupedByLocation = pawns.Where(p => p.ColorPlayer == pawn.ColorPlayer)
                                                 .GroupBy(p => p.Location)
                                                 .ToList();

                    foreach (var group in groupedByLocation)
                    {
                        Point location = group.Key;
                        int countSameColor = group.Count();

                        if (countSameColor > 1)
                        {
                            var existingLabel = this.Controls.OfType<Label>()
                                .FirstOrDefault(l => l.Name == "countLabel_" + location.X + "_" + location.Y);

                            if (existingLabel == null)
                            {
                                Label countOnField = new Label();
                                countOnField.Name = "countLabel_" + location.X + "_" + location.Y;
                                countOnField.Location = new Point(location.X + 10, location.Y); 
                                countOnField.Text = "x" + countSameColor;
                                countOnField.AutoSize = true;
                                countOnField.BackColor = Color.Transparent;

                                this.Controls.Add(countOnField);
                                countOnField.BringToFront();
                            }
                            else
                            {
                                existingLabel.Text = "x" + countSameColor;
                            }
                        }
                    }
                }
            }
        }




        private Point Winning(Pawn selectedPawn, int steps, Point currentPoint)
        {
            Point loc = new Point();
            loc = currentPoint;
            bool allSameColorNotActive = allPawns
.SelectMany(list => list)
.Where(pawn => pawn.ColorPlayer == selectedPawn.ColorPlayer && pawn != selectedPawn)
.All(pawn => pawn.IsActive == false);
            if (steps > selectedPawn.HowManyToWin)
            { 
                return loc;
            }
            else
            {
                while (steps > 0)
                {
                    switch (selectedPawn.ColorPlayer)
                    {
                        case "blue":
                            loc = MoveUp(loc);
                            break;
                        case "yellow":
                            loc = MoveLeft(loc);
                            break;
                        case "green":
                            loc = MoveDown(loc);
                            break;
                        case "red":
                            loc = MoveRight(loc);
                            break;
                        default:
                            throw new Exception("Invalid pawn color!");
                    }
                    selectedPawn.Location = loc; 
                    steps--;
                    selectedPawn.HowManyToWin--;
                }
                selectedPawn.OriginalField = loc;
            }
            if (selectedPawn.HowManyToWin == 0)
            {
                selectedPawn.IsActive = false;
                MessageBox.Show("Congratulations! Your pawn is on meta!");
            }
            if (AreAllPawnsAtLocation(selectedPawn.ColorPlayer, loc))
            {
                MessageBox.Show("Congratulations! You took: " + (place + 1) + " place!");
                places[place].Text = (place + 1) + " place: " + selectedPawn.ColorPlayer;
                place++;
            }
            if(place == 3)
            {
                string color = ""; 

                // Find the color of the pawn whose .HowManyToWin != 0
                foreach (var pawnList in allPawns)
                {
                    foreach (var pawn in pawnList)
                    {
                        if (pawn.HowManyToWin != 0)
                        {
                            color = pawn.ColorPlayer;
                            break; // Finish after finding the first suitable pawn
                        }
                    }
                    if (!string.IsNullOrEmpty(color))
                        break; // End the loop when you find the first suitable pawn
                }
                places[place].Text = (place + 1) + " place: " + color;
                place++;
            }
            return loc;
        }
        private bool AreAllPawnsAtLocation(string color, Point loc)
        {
            // Find a list of pieces of a given color
            var pawnsOfColor = allPawns.FirstOrDefault(pawns => pawns.Any(pawn => pawn.ColorPlayer == color));

            if (pawnsOfColor != null && pawnsOfColor.All(pawn => pawn.Location == loc))
            {
                return true;
            }

            return false;
        }
        private void CollisionAndCapturing(Pawn selectedPawn)
        {
            var pawnsAtSameField = allPawns.SelectMany(pawns => pawns.Where(pawn => pawn.OriginalField == selectedPawn.OriginalField));
           
            if (pawnsAtSameField.All(pawn => pawn.ColorPlayer == selectedPawn.ColorPlayer)) 
            {
                foreach (var pawn in pawnsAtSameField)
                {
                    pawn.Location = pawn.OriginalField;
                }
            }
            if (allPawns.Any(pawns =>
                    pawns.Any(pawn => pawn.OriginalField == selectedPawn.OriginalField && pawn.ColorPlayer != selectedPawn.ColorPlayer)))
            {
                selectedPawn.OriginalField = selectedPawn.Location;
                selectedPawn.IsChanged = true;



                // Check if there is a pawn of the same color with the same OriginalField
                var sameColorPawns = pawnsAtSameField.Where(pawn => (pawn.ColorPlayer == selectedPawn.ColorPlayer && pawn.Location != selectedPawn.Location)).ToList();

                if (sameColorPawns.Count > 0)
                {
                    var matchingPawn = sameColorPawns.First();
                    selectedPawn.Location = matchingPawn.Location;

                    return;
                }
                
                var uniqueColors = pawnsAtSameField.Select(pawn => pawn.ColorPlayer).Distinct().ToList();
                int uniqueColorsCount = uniqueColors.Count;
                int howManyPawns = pawnsAtSameField.Count();
                
                foreach (var pawn in pawnsAtSameField)
                {
                    pawn.IsChanged = true;
                    if (uniqueColorsCount == 2)
                    {
                        if (howManyPawns == 2)
                        {
                            Pawn pawnToCapture = pawnsAtSameField.FirstOrDefault(p => p.ColorPlayer != selectedPawn.ColorPlayer);
                            CapturingPawn(selectedPawn, pawnToCapture);
                            return;
                        }
                        else
                        {
                            if (pawn != selectedPawn)
                            {
                                Point color1 = new Point();
                                color1.X = pawn.OriginalField.X - 10;
                                color1.Y = pawn.OriginalField.Y - 10;
                                pawn.Location = color1;
                            }
                            else
                            {
                                Point color2 = new Point();
                                color2.X = selectedPawn.OriginalField.X + 10;
                                color2.Y = selectedPawn.OriginalField.Y + 10;
                                selectedPawn.Location = color2;
                                Pawn pawnToCapture = pawnsAtSameField.FirstOrDefault(p => p.ColorPlayer != selectedPawn.ColorPlayer);
                                CapturingPawn(selectedPawn, pawnToCapture);
                            }
                        }

                    }
                    else if (uniqueColorsCount == 3)
                    {
                        Point color3 = new Point();
                        color3.X = pawn.OriginalField.X - 10;
                        color3.Y = pawn.OriginalField.Y + 10;
                        selectedPawn.Location = color3;

                        CapturingMany(selectedPawn, pawnsAtSameField);
                        break;
                    }
                    else if (uniqueColorsCount == 4)
                    {
                        Point color4 = new Point();
                        color4.X = pawn.OriginalField.X + 10;
                        color4.Y = pawn.OriginalField.Y - 10;
                        selectedPawn.Location = color4;

                        CapturingMany(selectedPawn, pawnsAtSameField);
                        break;
                    }
                }

            } 

        }
        private void ReturnToOriginalField(Pawn selectedPawn)
        {
            var pawnsAtSameField = allPawns.SelectMany(pawns => pawns.Where(pawn => (pawn.OriginalField == selectedPawn.OriginalField) && pawn!= selectedPawn));

            if (pawnsAtSameField.Any())
            {
                // Sprawdź, czy wszystkie pionki mają ten sam kolor
                var firstPawnColor = pawnsAtSameField.First().ColorPlayer;
                bool allSameColor = pawnsAtSameField.All(pawn => pawn.ColorPlayer == firstPawnColor);

                if (allSameColor)
                {
                    foreach (var pawn in pawnsAtSameField)
                    {
                        pawn.Location = pawn.OriginalField;
                    }
                }

            }
            
        }

        private void CapturingMany(Pawn selectedPawn, IEnumerable<Pawn> pawnsWithSameOriginalField)
        {
            string selectedColor = "";
            List<string> colorsWithSameOriginalField = pawnsWithSameOriginalField
                .Where(pawn => pawn.ColorPlayer != selectedPawn.ColorPlayer)
                .Select(pawn => pawn.ColorPlayer)
                .Distinct()
                .ToList();


            bool validSelection = false;
            while (!validSelection)
            {
                using (Message_ComboBox form = new Message_ComboBox(colorsWithSameOriginalField))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        selectedColor = form.SelectedValue;
                        validSelection = true;
                    }
                    else
                    {
                        MessageBox.Show("Selection canceled. The selection window will appear again, you need to select some color...");

                    }
                }
            }

            Pawn pawnToCapture = pawnsWithSameOriginalField
                .FirstOrDefault(pawn => pawn.ColorPlayer == selectedColor) ?? default;

            CapturingPawn(selectedPawn, pawnToCapture);

        }

    }
}