using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics; 

namespace NPRG2_Shooter
{
    class Animation
    {
        Texture2D spriteStrip; // collection of images used for animation
        float scale; // The scale used to display the sprite strip
        int elapsedTime; // The time since we last updated the frame
        int frameTime; // The time we display a frame until the next one
        int frameCount; // The number of frames that the animation contains
        int currentFrame; // The index of the current frame we are displaying
        Color color; // The color of the frame we will be displaying
        Rectangle sourceRect = new Rectangle(); // The area of the image strip we want to display
        Rectangle destinationRect = new Rectangle(); // The area where we want to display the image strip in the game
        public int FrameWidth; // Width of a given frame
        public int FrameHeight; // Height of a given frame
        public bool Active; // The state of the Animation
        public bool Looping; // Determines if the animation will keep playing or deactivate after one run
        public Vector2 Position; // Width of a given frame

        // "Constructor"
        public void Initialize(Texture2D texture, Vector2 position, int frameWidth,
         int frameHeight, int frameCount, int frametime, Color color, float scale, bool looping)
        {
            // Keep a local copy of the values passed in
            this.color = color;
            this.FrameWidth = frameWidth;
            this.FrameHeight = frameHeight;
            this.frameCount = frameCount;
            this.frameTime = frametime;
            this.scale = scale;

            Looping = looping;
            Position = position;
            spriteStrip = texture;

            // Set the time to zero
            elapsedTime = 0;
            currentFrame = 0;

            // Set the Animation to active by default
            Active = true;
        }

        public void Update(GameTime gameTime)
        {
            if (Active == false) return; // if not active we do nothing
            elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (elapsedTime > frameTime) // we need to switch frames
            {
                currentFrame++;
                if (currentFrame == frameCount)  // are we at the end?
                {                                // not to fall off of the end
                    currentFrame = 0;
                    if (Looping == false)        // make sure to end if appropriate
                    {
                        Active = false;
                    }
                }
                elapsedTime = 0;
            }
            // cut the rectangle from sprite and calculate the position of player to draw onto
            sourceRect = new Rectangle(currentFrame * FrameWidth, 0, FrameWidth, FrameHeight);
            destinationRect = new Rectangle((int)Position.X - (int)(FrameWidth * scale) / 2,
                                            (int)Position.Y - (int)(FrameHeight * scale) / 2,
                                            (int)(FrameWidth * scale), (int)(FrameHeight * scale));
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            // Only draw the animation when we are active
            if (Active) {
                spriteBatch.Draw(spriteStrip, destinationRect, sourceRect, color);
            }
        }
    }
}
