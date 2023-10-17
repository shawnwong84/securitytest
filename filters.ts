import { Listing } from "./types";

/**
 * A function which takes in a Listing, and depending on its conditions, returns a boolean on whether or not the Listing passes the filter
 */
type FilterFunction = (listing: Listing) => boolean;

/**
 * Main filtering function that will accept an array of listings, and return a list of filtered listings
 * @param listings An array of Listing objects
 * @param filters An array of FilterFunctions
 * @returns
 */
export const filterListings = (listings: Listing[], filters: FilterFunction[]) => {
  return listings.filter((listing) =>
    filters.every((filter) => filter(listing))
  );
};

/**
 * Checks if a listing is within a certain price range
 * @param listing The Listing object
 * @param minPrice The minimum price a listing can be, 0 if not defined, inclusive
 * @param maxPrice The maximum price a listing can be, unlimited if not defined, inclusive
 * @returns 
 */
export const filterByPrice = (minPrice: number = 0, maxPrice: number = Infinity): FilterFunction => {
  return (listing: Listing) => listing.price >= minPrice && listing.price <= maxPrice;
}

/**
 * Checks if a listing is within a posted within a certain recentness
 * @param listing The Listing object
 * @param timeSincePostedSeconds The maximum amount of time since the listing of posted
 * @returns 
 */
export const filterByElaspedTime = (timeSincePostedSeconds: number = Infinity): FilterFunction => {
  return (listing: Listing) => listing.price <= timeSincePostedSeconds;
}

/**
 * Checks if a listing's title has or doesn't have certain keywords (case insensitive)
 * @param listing The Listing object
 * @param mustInclude An array of strings that MUST be in a listing's title (recommended to be the same as your search term)
 * @param maxPrice An array of strings that MUST NOT be in a listing's title 
 * @returns 
 */
export const filterByKeywords = (mustInclude: string[], mustExclude: string[]): FilterFunction => {
  return (listing: Listing) => {
    
    // Check if the listing's title does not contain any of the "mustExclude" keywords
    // Perform a whole-word match
    function containsBlacklistedWord(blacklist: string[], givenString: string): boolean {
      // Split the given string into words using whitespace as the separator
      const words = givenString.split(/\s+/);
    
      // Iterate through each word in the blacklist
      for (const blacklistedWord of blacklist) {
        // Check if the blacklistedWord is included in the words array as a whole word
        if (words.includes(blacklistedWord)) {
          return true; // Found a blacklisted word
        }
      }
    
      return false; // None of the blacklisted words were found
    }

    const title = listing.name.toLowerCase(); // Convert title to lowercase for case-insensitive comparison

    // Check if the listing's title contains all the "mustInclude" keywords
    // Substring match
    const includesAllKeywords = mustInclude.every(keyword => title.includes(keyword.toLowerCase()));

    // Return true if the listing meets both conditions
    return includesAllKeywords && !containsBlacklistedWord(mustExclude, title);
  };
}
