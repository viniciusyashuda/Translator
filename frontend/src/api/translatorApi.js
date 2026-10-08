const API_BASE_URL = 'http://localhost:5225/api/translation';

export async function checkHealth() {
  try {
    const response = await fetch(`${API_BASE_URL}/health`);
    if (!response.ok) {
      return { status: 'Offline', healthy: false };
    }
    const data = await response.json();
    return { ...data, healthy: true };
  } catch (error) {
    return { status: 'Offline', healthy: false, error: error.message };
  }
}

export async function getLanguages() {
  try {
    const response = await fetch(`${API_BASE_URL}/languages`);
    if (!response.ok) {
      throw new Error(`Failed to load languages: ${response.status}`);
    }
    return await response.json();
  } catch (error) {
    return [
      { code: 'auto', name: 'Auto Detect', nativeName: 'Detectar idioma', flag: '🌐' },
      { code: 'en', name: 'English', nativeName: 'English', flag: '🇺🇸' },
      { code: 'pt', name: 'Portuguese', nativeName: 'Português', flag: '🇧🇷' },
      { code: 'es', name: 'Spanish', nativeName: 'Español', flag: '🇪🇸' },
      { code: 'fr', name: 'French', nativeName: 'Français', flag: '🇫🇷' },
      { code: 'de', name: 'German', nativeName: 'Deutsch', flag: '🇩🇪' },
      { code: 'it', name: 'Italian', nativeName: 'Italiano', flag: '🇮🇹' },
      { code: 'ja', name: 'Japanese', nativeName: '日本語', flag: '🇯🇵' },
      { code: 'zh', name: 'Chinese', nativeName: '中文', flag: '🇨🇳' },
    ];
  }
}

export async function getPresets() {
  try {
    const response = await fetch(`${API_BASE_URL}/presets`);
    if (!response.ok) {
      throw new Error(`Failed to load presets: ${response.status}`);
    }
    return await response.json();
  } catch (error) {
    return [
      {
        id: 'preset-1',
        sourceLanguage: 'pt',
        targetLanguage: 'en',
        text: "O projeto deu com os burros n'água no final do sprint.",
        category: 'Idiom / Slang',
      },
      {
        id: 'preset-2',
        sourceLanguage: 'pt',
        targetLanguage: 'en',
        text: 'Depois daquela reunião difícil, decidi chutar o balde e tirar férias.',
        category: 'Idiom / Slang',
      },
      {
        id: 'preset-3',
        sourceLanguage: 'en',
        targetLanguage: 'pt',
        text: "Break a leg tonight! I know you've been burning the midnight oil for this demo.",
        category: 'Idiom / Slang',
      },
      {
        id: 'preset-4',
        sourceLanguage: 'en',
        targetLanguage: 'pt',
        text: 'We need to bite the bullet and cut our losses before it costs an arm and a leg.',
        category: 'Business / Nuance',
      },
    ];
  }
}

export async function translateAndExplain(request) {
  const response = await fetch(API_BASE_URL, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  });

  const data = await response.json();

  if (!response.ok) {
    const errorMessage = data.detail || data.title || (data.errors ? Object.values(data.errors).flat().join(', ') : 'Translation failed');
    throw new Error(errorMessage);
  }

  return data;
}
