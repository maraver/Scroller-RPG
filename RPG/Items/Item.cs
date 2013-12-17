using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using RPG.Entities;
using RPG.Sprites;
using RPG.Screen;

namespace RPG.Items
{
    [Serializable]
    public class Item : ISerializable
    {
        public const int SIZE = 24, MAX_STACK = 64;

        public readonly string Name;
        public readonly ItemId Id;
        public readonly Sprite Sprite;
        public readonly Action<Player, Item> UseFunc;
        public readonly bool Copy, Stackable;

        public int Count { get; private set; }

        public Item(ItemId id, string name, Dictionary<ItemId, Armour> armours, Action<Player, Item> useFunc=null)
            : this(id, name, armours[id].Stand, useFunc, false) { }

        public Item(ItemId id, string name, Texture2D tex, Action<Player, Item> useFunc, bool stackable=false) {
            this.Name = name;
            this.Id = id;
            this.Sprite = new Sprite(tex);
            this.UseFunc = (useFunc != null) ? useFunc : ItemEvents.NoAction;
            this.Stackable = stackable;
            this.Count = 1;
            this.Copy = false;
        }

        public Item(Item i, String name=null) {
            if (name != null) this.Name = name;
            else this.Name = i.Name;
            this.Id = i.Id;
            this.Sprite = i.Sprite.Clone();
            this.UseFunc = i.UseFunc;
            this.Stackable = i.Stackable;
            this.Count = 1;
            this.Copy = true;
        }

        // Serialize
        public Item(SerializationInfo info, StreamingContext cntxt) 
        : this(GameScreen.Items[(ItemId) info.GetValue("Item_Id", typeof(ItemId))], (string) info.GetValue("Item_Name", typeof(string)))
        { 
            this.Count = (int) info.GetValue("Item_Count", typeof(int));
            this.Copy = (bool) info.GetValue("Item_Copy", typeof(bool));
            this.Stackable = (bool) info.GetValue("Item_Stackable", typeof(bool));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext cntxt) {
            info.AddValue("Item_Id", Id);
            info.AddValue("Item_Name", Name);
            info.AddValue("Item_Count", Count);
            info.AddValue("Item_Copy", Copy);
            info.AddValue("Item_Stackable", Stackable);
        }

        public Item Clone() {
            return new Item(this);
        }

        /// <summary>
        /// Increases the count of this item's stack. Can't be used to remove from stack.
        /// </summary>
        /// <param name="amnt">The amount to add</param>
        /// <returns>If there are extra, an item stack with them</returns>
        public Item addToStack(int amnt) {
            if (isStackable() && amnt > 0) {
                Count += amnt;
                if (Count > MAX_STACK) {
                    Item rtnItem = new Item(this);
                    // Initialized with a size of one so -1
                    rtnItem.addToStack(Count - MAX_STACK - 1);
                    Count = MAX_STACK;
                    return rtnItem;
                }
            }
            return null;
        }

        public Item removeFromStack(int amnt) {
            if (isStackable() && amnt > 0) {
                if (amnt >= Count) amnt = Count - 1;
                Count -= amnt;
                Item newItem = new Item(this);
                newItem.addToStack(amnt - 1);
                return newItem;
            } else {
                return null;
            }
        }

        public override string ToString() {
            string str = Name;
            if (Stackable) str += " (" + Count.ToString() + ")";
            return str;
        }

        public bool isStackable() {
            return (Copy && Stackable);
        }
    }
}
