using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel;
using ludo_game;

[Designer(typeof(ParentControlDesigner))]
public class Pawn : PictureBox
{
    public string ColorPlayer { get; }
    public Point LastChangeDirectionPoint { get; set; }
    public Point SafePosition { get; set; }

    public Point InitialPosition { get; private set; }
    public Point BeginMeta { get; set; }
    public Point OriginalField { get; set; }
    public int HowManyToWin { get; set; } = 6;
    public bool WasAtBeginMeta { get; set; } = false;

    public bool IsChanged = false;
    public bool IsActive { get; set; }

    public Pawn(Image image, string color, Point initialPosition)
    {
        Image = image;
        ColorPlayer = color;
        InitialPosition = initialPosition; 
        Location = initialPosition;
        Size = new Size(10, 16); 
        BackColor = Color.Transparent;
        SizeMode = PictureBoxSizeMode.StretchImage;
        IsActive = false;

    }
}
