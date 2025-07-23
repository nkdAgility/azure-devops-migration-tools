// Cache to store version data with timestamps
const versionCache = new Map();
const CACHE_DURATION = 10 * 60 * 1000; // 10 minutes in milliseconds

/**
 * Fetches version information from the version.json endpoint
 * @param {string} url - The full URL to version.json (e.g., 'https://devopsmigration.io/version.json')
 * @returns {Promise<string>} - The version string
 */
async function getVersion(url) {
  // Check cache first
  const cached = versionCache.get(url);
  if (cached && Date.now() - cached.timestamp < CACHE_DURATION) {
    console.log("Returning cached version for:", url);
    return cached.version;
  }

  try {
    console.log("Fetching fresh version from:", url);
    const response = await fetch(url);

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    const data = await response.json();

    // Cache the result
    versionCache.set(url, {
      version: data.version,
      timestamp: Date.now(),
    });

    return data.version;
  } catch (error) {
    console.error("Error fetching version:", error);
    throw error;
  }
}

// Export functions for use in modules or other scripts
if (typeof module !== "undefined" && module.exports) {
  module.exports = {
    getVersion,
  };
}
