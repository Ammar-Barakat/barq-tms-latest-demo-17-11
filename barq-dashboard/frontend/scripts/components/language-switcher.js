/**
 * Language Switcher Component
 * Provides UI for switching between languages
 */

class LanguageSwitcher {
  constructor() {
    this.currentLanguage = window.i18n?.getCurrentLanguage() || "en";
    this.languages = {
      en: { name: "English", flag: "🇬🇧", dir: "ltr" },
      ar: { name: "العربية", flag: "🇸🇦", dir: "rtl" },
    };
  }

  /**
   * Initialize the language switcher
   */
  initialize() {
    console.log("Language switcher initializing...");

    // Create language switcher if not exists in header
    this.createLanguageSwitcher();

    // Listen to language change events
    window.addEventListener("languageChanged", (e) => {
      this.currentLanguage = e.detail.language;
      this.updateUI();
    });

    console.log("Language switcher initialized");
  }

  /**
   * Create language switcher UI in header
   */
  createLanguageSwitcher() {
    const headerRight = document.querySelector(".header-right");
    console.log("Header right element:", headerRight);
    console.log(
      "Existing language switcher:",
      document.querySelector(".language-switcher")
    );

    if (!headerRight) {
      console.warn("Header right element not found!");
      return;
    }

    if (document.querySelector(".language-switcher")) {
      console.log("Language switcher already exists");
      return;
    }

    const currentLang = this.languages[this.currentLanguage];

    const switcherHTML = `
      <button class="header-btn language-switcher" id="languageBtn" title="Change Language">
        <span class="language-flag">${currentLang.flag}</span>
        <span class="language-code">${this.currentLanguage.toUpperCase()}</span>
      </button>
      <div class="language-switcher-dropdown" id="languageDropdown">
        ${Object.entries(this.languages)
          .map(
            ([code, lang]) => `
          <div class="language-option ${
            code === this.currentLanguage ? "active" : ""
          }" 
               data-language="${code}">
            <span class="language-flag">${lang.flag}</span>
            <span class="language-name">${lang.name}</span>
          </div>
        `
          )
          .join("")}
      </div>
    `;

    // Insert before notification button or user menu
    const notificationBtn = headerRight.querySelector(".notification-btn");
    if (notificationBtn) {
      console.log("Inserting language switcher before notification button");
      notificationBtn.insertAdjacentHTML("beforebegin", switcherHTML);
    } else {
      console.log("Inserting language switcher at beginning of header-right");
      headerRight.insertAdjacentHTML("afterbegin", switcherHTML);
    }

    console.log("Language switcher HTML inserted");
    this.attachEventListeners();
  }

  /**
   * Attach event listeners
   */
  attachEventListeners() {
    const languageBtn = document.getElementById("languageBtn");
    const languageDropdown = document.getElementById("languageDropdown");
    const languageOptions = document.querySelectorAll(".language-option");

    if (!languageBtn || !languageDropdown) return;

    // Toggle dropdown
    languageBtn.addEventListener("click", (e) => {
      e.stopPropagation();
      const isShowing = languageDropdown.classList.contains("show");
      languageDropdown.classList.toggle("show");

      // Position dropdown relative to button
      if (!isShowing) {
        const btnRect = languageBtn.getBoundingClientRect();
        languageDropdown.style.top = `${btnRect.bottom + 5}px`;
        languageDropdown.style.right = `${window.innerWidth - btnRect.right}px`;
      }
    });

    // Close dropdown when clicking outside
    document.addEventListener("click", (e) => {
      if (
        !e.target.closest(".language-switcher") &&
        !e.target.closest(".language-switcher-dropdown")
      ) {
        languageDropdown.classList.remove("show");
      }
    });

    // Handle language selection
    languageOptions.forEach((option) => {
      option.addEventListener("click", async (e) => {
        const language = option.getAttribute("data-language");
        if (language !== this.currentLanguage) {
          try {
            await window.i18n.switchLanguage(language);
            languageDropdown.classList.remove("show");

            // Show success message
            if (window.utils?.showToast) {
              const langName = this.languages[language].name;
              window.utils.showToast(
                `Language changed to ${langName}`,
                "success"
              );
            }
          } catch (error) {
            console.error("Failed to switch language:", error);
            if (window.utils?.showToast) {
              window.utils.showToast("Failed to switch language", "error");
            }
          }
        }
      });
    });
  }

  /**
   * Update UI after language change
   */
  updateUI() {
    const languageBtn = document.getElementById("languageBtn");
    const languageOptions = document.querySelectorAll(".language-option");

    if (languageBtn) {
      const currentLang = this.languages[this.currentLanguage];
      languageBtn.querySelector(".language-flag").textContent =
        currentLang.flag;
      languageBtn.querySelector(".language-code").textContent =
        this.currentLanguage.toUpperCase();
    }

    // Update active state
    languageOptions.forEach((option) => {
      const lang = option.getAttribute("data-language");
      option.classList.toggle("active", lang === this.currentLanguage);
    });
  }

  /**
   * Get current language
   */
  getCurrentLanguage() {
    return this.currentLanguage;
  }
}

// Create global instance
window.languageSwitcher = new LanguageSwitcher();

// Initialize when both DOM and i18n are ready
function initLanguageSwitcher() {
  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", () => {
      setTimeout(() => window.languageSwitcher.initialize(), 100);
    });
  } else {
    setTimeout(() => window.languageSwitcher.initialize(), 100);
  }
}

// Initialize after a short delay to ensure DOM is ready
initLanguageSwitcher();
