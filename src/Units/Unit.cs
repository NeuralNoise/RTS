using System;

public abstract class Unit {
    private Game p_Game;
    private UnitManager p_Manager;
    private int p_X, p_Y;
    private int p_W, p_H;
    private int p_RW, p_RH;
    private int p_SpriteID;

    private int p_HP;

    public Unit(Game game, UnitManager manager, int width, int height) {
        p_Game = game;
        p_Manager = manager;

        //check the size
        if (width <= 0 || height <= 0) {
            throw new Exception("Size cannot be zero or negative");
        }
        Camera cam = game.Camera;
        p_W = width;
        p_H = height;
        p_RW = width * cam.BlockWidth;
        p_RH = height * cam.BlockHeight;
    }

    public void Hit(int amount) {
        if (amount < 0) { amount = -amount; }
        p_HP -= amount;
        if (p_HP <= 0) {
            p_HP = 0;

            
        }
    }
    public void Heal(int amount) {
        if (amount < 0) { amount = -amount; }
        p_HP += amount;
        if (p_HP >= MaxHP) {
            p_HP = MaxHP;
        }
    }

    public void GetBlockLocation(out int x, out int y) {
        Camera cam = p_Game.Camera;
        x = (int)Math.Floor(p_X * 1.0f / cam.BlockWidth);
        y = (int)Math.Floor(p_Y * 1.0f / cam.BlockHeight);
    }

    public int X { get { return p_X; } set { p_X = value; } }
    public int Y { get { return p_Y; } set { p_Y = value; } }

    public int Width { get { return p_W; } }
    public int Height { get { return p_H; } }
    public int RenderWidth { get { return p_RW; } }
    public int RenderHeight { get { return p_RH; } }
    public int SpriteID { get { return p_SpriteID; } }

    public UnitManager Manager { get { return p_Manager; } }

    public int HP { get { return p_HP; } }
    public abstract int MaxHP { get; }
}