using System.Drawing;
using System.Numerics;


namespace UAI.UI
{

    public class PopupMenu
    {
        private List<string> items = new List<string>();

        /// <summary>
        /// Adds an item to the popup menu.
        /// </summary>
        /// <param name="itemName">The name of the item to add.</param>
        /// <param name="id">The ID of the item (optional).</param>
        public virtual void AddItem(string itemName, int id = -1)
        {
            items.Add(itemName);
            Console.WriteLine($"Added item '{itemName}' with ID {id} to the popup menu.");
        }

        /// <summary>
        /// Clears all items from the popup menu.
        /// </summary>
        public virtual void Clear()
        {
            items.Clear();
            Console.WriteLine("Popup menu cleared.");
        }

        /// <summary>
        /// Sets the position of the popup menu on the screen.
        /// </summary>
        /// <param name="position">The position as a Point.</param>
        public virtual void SetPosition(Vector2 position)
        {
            Console.WriteLine($"Popup menu set to position ({position.X}, {position.Y}).");
        }

        /// <summary>
        /// Displays the popup menu.
        /// </summary>
        public virtual void Popup()
        {
            Console.WriteLine("Popup menu displayed with the following items:");
        
        }
    }
}
