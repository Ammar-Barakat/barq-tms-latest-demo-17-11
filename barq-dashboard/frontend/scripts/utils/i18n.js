/**
 * Internationalization (i18n) System
 * Handles language switching, translation loading, and RTL/LTR direction
 */

class I18n {
  constructor() {
    this.currentLanguage = localStorage.getItem("language") || "en";
    this.translations = {};
    this.rtlLanguages = ["ar", "he", "fa", "ur"]; // Languages that use RTL
    this.initialized = false;
  }

  /**
   * Initialize the i18n system
   */
  async initialize() {
    if (this.initialized) return;

    try {
      // Load translations for current language
      await this.loadTranslations(this.currentLanguage);

      // Apply language and direction to DOM
      this.applyLanguage(this.currentLanguage);

      // Setup language switcher if exists
      this.setupLanguageSwitcher();

      // Translate all elements on the page
      this.translatePage();

      this.initialized = true;
      console.log(`i18n initialized with language: ${this.currentLanguage}`);
    } catch (error) {
      console.error("Failed to initialize i18n:", error);
      // Fallback to English if initialization fails
      this.currentLanguage = "en";
      await this.loadTranslations("en");
      this.applyLanguage("en");
    }
  }

  /**
   * Load translations from JSON file
   */
  async loadTranslations(language) {
    try {
      // Try multiple paths to find the JSON file
      const paths = [
        `/${language}.json`, // From root
        `../../../${language}.json`, // Three levels up
        `../../${language}.json`, // Two levels up
        `./${language}.json`, // Same directory
      ];

      let response;
      let loadedPath;

      for (const path of paths) {
        try {
          response = await fetch(path);
          if (response.ok) {
            loadedPath = path;
            break;
          }
        } catch (e) {
          continue;
        }
      }

      if (!response || !response.ok) {
        throw new Error(`Failed to load ${language}.json from any path`);
      }

      this.translations[language] = await response.json();
      console.log(`Loaded translations for: ${language} from ${loadedPath}`);
    } catch (error) {
      console.error(`Error loading translations for ${language}:`, error);
      throw error;
    }
  }

  /**
   * Get translation for a key using dot notation
   * @param {string} key - Translation key (e.g., 'nav.dashboard', 'common.save')
   * @param {object} params - Optional parameters for interpolation
   * @returns {string} Translated text
   */
  t(key, params = {}) {
    const keys = key.split(".");
    let value = this.translations[this.currentLanguage];

    // Navigate through nested object
    for (const k of keys) {
      if (value && typeof value === "object") {
        value = value[k];
      } else {
        value = undefined;
        break;
      }
    }

    // Return key if translation not found
    if (value === undefined) {
      console.warn(`Translation not found for key: ${key}`);
      return key;
    }

    // Handle string interpolation
    if (typeof value === "string" && Object.keys(params).length > 0) {
      return value.replace(/\{(\w+)\}/g, (match, paramKey) => {
        return params[paramKey] !== undefined ? params[paramKey] : match;
      });
    }

    return value;
  }

  /**
   * Switch to a different language
   */
  async switchLanguage(language) {
    if (this.currentLanguage === language) return;

    try {
      // Load translations if not already loaded
      if (!this.translations[language]) {
        await this.loadTranslations(language);
      }

      this.currentLanguage = language;
      localStorage.setItem("language", language);

      // Apply language changes to DOM
      this.applyLanguage(language);

      // Translate all elements on the page
      this.translatePage();

      // Emit language change event
      window.dispatchEvent(
        new CustomEvent("languageChanged", {
          detail: { language },
        })
      );

      console.log(`Language switched to: ${language}`);
    } catch (error) {
      console.error("Failed to switch language:", error);
      throw error;
    }
  }

  /**
   * Apply language attributes and direction to DOM
   */
  applyLanguage(language) {
    const html = document.documentElement;
    const body = document.body;

    // Set language attribute
    html.setAttribute("lang", language);

    // Set direction (RTL/LTR)
    const isRTL = this.rtlLanguages.includes(language);
    html.setAttribute("dir", isRTL ? "rtl" : "ltr");
    body.classList.toggle("rtl", isRTL);
    body.classList.toggle("ltr", !isRTL);

    // Update meta tag if exists
    let metaLang = document.querySelector('meta[name="language"]');
    if (!metaLang) {
      metaLang = document.createElement("meta");
      metaLang.setAttribute("name", "language");
      document.head.appendChild(metaLang);
    }
    metaLang.setAttribute("content", language);
  }

  /**
   * Translate all elements with data-i18n attribute
   */
  translatePage() {
    // Translate elements with data-i18n attribute
    const elements = document.querySelectorAll("[data-i18n]");
    elements.forEach((element) => {
      const key = element.getAttribute("data-i18n");
      const translation = this.t(key);

      // Handle different element types
      if (element.tagName === "INPUT" || element.tagName === "TEXTAREA") {
        if (element.hasAttribute("placeholder")) {
          element.setAttribute("placeholder", translation);
        } else {
          element.value = translation;
        }
      } else {
        element.textContent = translation;
      }
    });

    // Translate elements with data-i18n-placeholder attribute
    const placeholderElements = document.querySelectorAll(
      "[data-i18n-placeholder]"
    );
    placeholderElements.forEach((element) => {
      const key = element.getAttribute("data-i18n-placeholder");
      element.setAttribute("placeholder", this.t(key));
    });

    // Translate elements with data-i18n-title attribute
    const titleElements = document.querySelectorAll("[data-i18n-title]");
    titleElements.forEach((element) => {
      const key = element.getAttribute("data-i18n-title");
      element.setAttribute("title", this.t(key));
    });

    // Translate elements with data-i18n-aria-label attribute
    const ariaLabelElements = document.querySelectorAll(
      "[data-i18n-aria-label]"
    );
    ariaLabelElements.forEach((element) => {
      const key = element.getAttribute("data-i18n-aria-label");
      element.setAttribute("aria-label", this.t(key));
    });
  }

  /**
   * Setup language switcher dropdown/buttons
   */
  setupLanguageSwitcher() {
    // Find language switcher elements
    const languageSwitchers = document.querySelectorAll(
      "[data-language-switcher]"
    );

    languageSwitchers.forEach((switcher) => {
      switcher.addEventListener("click", (e) => {
        e.preventDefault();
        const targetLang = switcher.getAttribute("data-language-switcher");
        this.switchLanguage(targetLang);
      });

      // Mark current language as active
      if (
        switcher.getAttribute("data-language-switcher") === this.currentLanguage
      ) {
        switcher.classList.add("active");
      }
    });

    // Update active state on language change
    window.addEventListener("languageChanged", (e) => {
      languageSwitchers.forEach((switcher) => {
        const lang = switcher.getAttribute("data-language-switcher");
        switcher.classList.toggle("active", lang === e.detail.language);
      });
    });
  }

  /**
   * Check if current language is RTL
   */
  isRTL() {
    return this.rtlLanguages.includes(this.currentLanguage);
  }

  /**
   * Get current language
   */
  getCurrentLanguage() {
    return this.currentLanguage;
  }

  /**
   * Get all available languages
   */
  getAvailableLanguages() {
    return ["en", "ar"];
  }

  /**
   * Format number based on current locale
   */
  formatNumber(number, options = {}) {
    const locale = this.currentLanguage === "ar" ? "ar-SA" : "en-US";
    return new Intl.NumberFormat(locale, options).format(number);
  }

  /**
   * Format date based on current locale
   */
  formatDate(date, options = {}) {
    const locale = this.currentLanguage === "ar" ? "ar-SA" : "en-US";
    return new Intl.DateTimeFormat(locale, options).format(date);
  }

  /**
   * Format currency based on current locale
   */
  formatCurrency(amount, currency = "USD") {
    const locale = this.currentLanguage === "ar" ? "ar-SA" : "en-US";
    return new Intl.NumberFormat(locale, {
      style: "currency",
      currency: currency,
    }).format(amount);
  }
}

// Create global instance
window.i18n = new I18n();

// Auto-initialize when DOM is ready
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", () => {
    window.i18n.initialize();
  });
} else {
  window.i18n.initialize();
}
