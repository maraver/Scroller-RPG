using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RPG.Entities;
using RPG.Screen;

namespace RPG.Items
{
    class ItemEvents
    {
        public static void NoAction(Player p, Item i) { }

        public static void NewRoom(Player p, Item i) { 
            p.gameScreen.newMainRoom(); 
            p.removeItem(i); 
        }

        public static void UseSmallPotion(Player p, Item i) { 
            p.heal(150); 
            p.removeItem(i); 
        }

        public static void Equip(Player p, Item i) {
            // Create a merged armour between a base armour (sprites, mult, ect.) and the item (name)
            Armour a = new Armour(GameScreen.Armours[i.Id], i);
            if (a != null) {
                p.removeItem(i);
                switch (a.Part) {
                    case ArmourParts.Head:
                        p.addItem(p.equipment.Head.Item);
                        p.equipment.setHead(a);
                        break;
                    case ArmourParts.Body:
                        p.addItem(p.equipment.Body.Item);
                        p.equipment.setBody(a);
                        break;
                    case ArmourParts.Legs:
                        p.addItem(p.equipment.Legs.Item);
                        p.equipment.setLegs(a);
                        break;
                }
            }
        }
    }
}
