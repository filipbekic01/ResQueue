namespace Resqueue
{
    public static class DefaultUserNames
    {
        private static readonly List<string> Adjectives =
        [
            "Mysterious", "Curious", "Silent", "Hidden", "Quiet",
            "Stealthy", "Unknown", "Invisible", "Anonymous", "Covert",
            "Masked", "Shy", "Shadow", "Enigmatic", "Veiled",
            "Cryptic", "Secretive", "Cloaked", "Disguised", "Ghostly",
            "Unseen", "Unidentified", "Concealed", "Mystic", "Nameless",
            "Phantom", "Shrouded", "Secretive", "Stealthy", "Veiled",
            "Shadowy", "Cryptic", "Invisible", "Silent", "Hidden",
            "Masked", "Covert", "Anonymous", "Cloaked", "Disguised",
            "Ghostly", "Unseen", "Mystic", "Secretive", "Shy",
            "Enigmatic", "Veiled", "Phantom", "Shadowy", "Cryptic",
            "Ethereal", "Luminous", "Radiant", "Celestial", "Obscure",
            "Twilight", "Eternal", "Frosty", "Glimmering", "Hushed",
            "Iridescent", "Jaded", "Kinetic", "Luminescent", "Mellow",
            "Nebulous", "Opalescent", "Prismatic", "Quiescent", "Radiant",
            "Serene", "Translucent", "Umbral", "Vibrant", "Whispering",
            "Xenial", "Yielding", "Zealous", "Amber", "Brisk",
            "Cobalt", "Dappled", "Emerald", "Faint", "Golden",
            "Hazel", "Icy", "Jubilant", "Keen", "Lush",
            "Majestic", "Nocturnal", "Opulent", "Pastel", "Quartz",
            "Rosy", "Sapphire", "Turquoise", "Ultramarine", "Verdant",
            "Winsome", "Youthful", "Zesty", "Aromatic", "Blushing",
            "Crimson", "Dusky", "Effervescent", "Fiery", "Gleaming",
            "Honeyed", "Indigo", "Jasper", "Kiwi", "Lilac",
            "Misty", "Nimble", "Ochre", "Pale", "Quincy",
            "Ruby", "Sandy", "Tawny", "Umber", "Viridian",
            "Wistful", "Xanthic", "Yellowish", "Azure", "Breezy",
            "Coral", "Dawn", "Electric", "Frost", "Garnet",
            "Heather", "Ivory", "Jet", "Khaki", "Lavender",
            "Mint", "Navy", "Olive", "Pearl", "Quartz",
            "Rose", "Silver", "Topaz", "Ultraviole", "Vanilla",
            "Wheat", "Xeric", "Yellow", "Zinc", "Auburn",
            "Beige", "Carmine", "Denim", "Ebony", "Fawn",
            "Gold", "Honey", "Ivory", "Jade", "Kale",
            "Lemon", "Magenta", "Nectarine", "Ocher", "Periwinkle",
            "Quartz", "Russet", "Scarlet", "Taupe", "Umber",
            "Violet", "Wine", "Xanadu", "Yellow-green", "Zaffre",
            "Apricot", "Burgundy", "Cerulean", "Daffodil", "Ecru",
            "Fuchsia", "Ginger", "Honeydew", "Ice", "Jasmine",
            "Kiwi-green", "Lime", "Maroon", "Nickel", "Olive-drab",
            "Papaya", "Quartz", "Ruby-red", "Sage", "Teal",
            "Ube", "Vermilion", "Walnut", "Xylem", "Yellow-orange",
            "Zucchini", "Amber-red", "Burnt-sienna", "Chartreuse", "Dusty",
            "Eggplant", "Fern", "Glaucous", "Harlequin", "Isabelline",
            "Jet-black", "Kelly-green", "Lilac-purple", "Mauve", "Navy-blue",
            "Opal", "Puce", "Quartz-blue", "Ruby-pink", "Sapphire-blue",
            "Turquoise-blue", "Ultramarine-blue", "Viridian-green", "Wisteria", "Xanthan",
            "Yellow-tan", "Zucchini-green", "Alabaster", "Blush", "Celadon",
            "Dandelion", "Eminence", "Flaxen", "Gossamer", "Heliotrope",
            "Imperial", "Jacinth", "Keppel", "Lapis", "Moss",
            "Nile", "Onyx", "Palladium", "Quartz-pink", "Raspberry",
            "Sienna", "Titanium", "Ultramarine-purple", "Venetian", "Wheatish",
            "Xanthous", "Yellowish-green", "Zinnia"
        ];

        private static readonly List<string> Nouns =
        [
            "Voyager", "Wanderer", "Explorer", "Nomad", "Pathfinder",
            "Traveler", "Seeker", "Adventurer", "Navigator", "Pilgrim",
            "Pioneer", "Wayfarer", "Roamer", "Navigator", "Traveler",
            "Nomad", "Explorer", "Wayfarer", "Navigator", "Pilgrim",
            "Adventurer", "Seeker", "Nomad", "Pathfinder", "Wanderer",
            "Explorer", "Wayfarer", "Navigator", "Pilgrim", "Voyager",
            "Wanderer", "Adventurer", "Traveler", "Pathfinder", "Nomad",
            "Explorer", "Wayfarer", "Nomad", "Pioneer", "Voyager",
            "Nomad", "Wanderer", "Explorer", "Wayfarer", "Nomad",
            "Voyager", "Wayfarer", "Nomad", "Seeker", "Adventurer",
            "Explorer", "Traveler", "Pathfinder", "Roamer", "Traveler",
            "Navigator", "Pilgrim", "Pioneer", "Pathfinder", "Rover",
            "Journeyer", "Tracker", "Scout", "Trailblazer", "Drifter",
            "Globetrotter", "Migrant", "Sailor", "Mariner", "Seafarer",
            "Corsair", "Buccaneer", "Privateer", "Settler", "Trailwalker",
            "Waywalker", "Journeyman", "Odyssey", "Questor", "Trailseeker",
            "Vagabond", "Nomadic", "Explorer", "Pathfinder", "Adventurer",
            "Voyager", "Traveler", "Wanderer", "Seeker", "Rambler",
            "Wayfarer", "Roamer", "Peripatetic", "Transient", "Globetrotter",
            "Itinerant", "Nomad", "Drifter", "Hiker", "Backpacker",
            "Scout", "Trailblazer", "Peregrine", "Mariner", "Seeker",
            "Marauder", "Voyager", "Navigator", "Pilgrim", "Pioneer",
            "Pathfinder", "Rover", "Journeyer", "Tracker", "Sailor",
            "Mariner", "Seafarer", "Corsair", "Buccaneer", "Privateer",
            "Settler", "Trailwalker", "Waywalker", "Journeyman", "Odyssey",
            "Questor", "Trailseeker", "Vagabond", "Nomadic", "Explorer",
            "Pathfinder", "Adventurer", "Voyager", "Traveler", "Wanderer",
            "Seeker", "Rambler", "Wayfarer", "Roamer", "Peripatetic",
            "Transient", "Globetrotter", "Itinerant", "Nomad", "Drifter",
            "Hiker", "Backpacker", "Scout", "Trailblazer", "Peregrine",
            "Mariner", "Seeker", "Marauder", "Voyager", "Navigator",
            "Pilgrim", "Pioneer", "Pathfinder", "Rover", "Journeyer",
            "Tracker", "Sailor", "Mariner", "Seafarer", "Corsair",
            "Buccaneer", "Privateer", "Settler", "Trailwalker", "Waywalker",
            "Journeyman", "Odyssey", "Questor", "Trailseeker", "Vagabond",
            "Nomadic", "Explorer", "Pathfinder", "Adventurer", "Voyager",
            "Traveler", "Wanderer", "Seeker", "Rambler", "Wayfarer",
            "Roamer", "Peripatetic", "Transient", "Globetrotter", "Itinerant",
            "Nomad", "Drifter", "Hiker", "Backpacker", "Scout",
            "Trailblazer", "Peregrine", "Mariner", "Seeker", "Marauder",
            "Voyager", "Navigator", "Pilgrim", "Pioneer", "Pathfinder",
            "Rover", "Journeyer", "Tracker", "Sailor", "Mariner",
            "Seafarer", "Corsair", "Buccaneer", "Privateer", "Settler",
            "Trailwalker", "Waywalker", "Journeyman", "Odyssey", "Questor",
            "Trailseeker", "Vagabond", "Nomadic", "Explorer", "Pathfinder",
            "Adventurer", "Voyager", "Traveler", "Wanderer", "Seeker",
            "Rambler", "Wayfarer", "Roamer", "Peripatetic", "Transiant",
            "Globetrotter", "Itinerant", "Nomad", "Drifter", "Hiker",
            "Backpacker", "Scout", "Trailblazer", "Peregrine", "Mariner",
            "Seeker", "Marauder", "Voyager", "Navigator", "Pilgrim",
            "Pioneer", "Pathfinder", "Rover", "Journeyer", "Tracker",
            "Sailor", "Mariner", "Seafarer", "Corsair", "Buccaneer",
            "Privateer", "Settler", "Trailwalker", "Waywalker", "Journeyman"
        ];

        private static readonly Random RandomGenerator = new();

        public static string GetRandomName()
        {
            var adjective = Adjectives[RandomGenerator.Next(Adjectives.Count)];
            var noun = Nouns[RandomGenerator.Next(Nouns.Count)];
            return $"{adjective} {noun}";
        }
    }
}