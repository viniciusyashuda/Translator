import React, { useState, useEffect } from 'react';
import {
  View,
  ScrollView,
  StyleSheet,
  SafeAreaView,
  Text,
} from 'react-native';
import { colors } from './theme/colors';
import { translations } from './i18n/translations';
import {
  checkHealth,
  getLanguages,
  getPresets,
  translateAndExplain,
} from './api/translatorApi';
import { Header } from './components/Header';
import { LanguageSelector } from './components/LanguageSelector';
import { ToneSelector } from './components/ToneSelector';
import { QuickPresets } from './components/QuickPresets';
import { TranslationInput } from './components/TranslationInput';
import { TranslationOutput } from './components/TranslationOutput';
import { ExplanationPanel } from './components/ExplanationPanel';

export default function App() {
  const [uiLanguage, setUiLanguage] = useState('pt');
  const t = translations[uiLanguage] || translations.pt;

  const [backendStatus, setBackendStatus] = useState(null);
  const [languages, setLanguages] = useState([]);
  const [presets, setPresets] = useState([]);
  const [sourceLanguage, setSourceLanguage] = useState('pt');
  const [targetLanguage, setTargetLanguage] = useState('en');
  const [tone, setTone] = useState('Natural');
  const [inputText, setInputText] = useState(
    'Depois daquela reunião difícil, decidi chutar o balde e tirar férias.'
  );
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [result, setResult] = useState(null);

  useEffect(() => {
    async function initialize() {
      const health = await checkHealth();
      setBackendStatus(health);

      const langs = await getLanguages();
      setLanguages(langs);

      const presetList = await getPresets();
      setPresets(presetList);
    }

    initialize();
  }, []);

  const handleSwap = () => {
    if (sourceLanguage === 'auto') return;
    const temp = sourceLanguage;
    setSourceLanguage(targetLanguage);
    setTargetLanguage(temp);
  };

  const handleSelectPreset = (preset) => {
    setInputText(preset.text);
    setSourceLanguage(preset.sourceLanguage);
    setTargetLanguage(preset.targetLanguage);
    setError(null);
  };

  const handleTranslate = async () => {
    if (!inputText.trim()) {
      setError(t.errorEmptyText);
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const payload = {
        text: inputText,
        sourceLanguage,
        targetLanguage,
        tone,
      };
      const data = await translateAndExplain(payload);
      setResult(data);
    } catch (err) {
      setError(err.message || 'Error executing translation');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <SafeAreaView style={styles.safeArea}>
      <Header
        backendStatus={backendStatus}
        uiLanguage={uiLanguage}
        onToggleUiLanguage={setUiLanguage}
        t={t}
      />

      <ScrollView
        style={styles.scrollView}
        contentContainerStyle={styles.contentContainer}
        showsVerticalScrollIndicator={false}
      >
        <View style={styles.mainContainer}>
          <View style={styles.translatorCard}>
            <View style={styles.cardHeader}>
              <LanguageSelector
                languages={languages}
                sourceLanguage={sourceLanguage}
                targetLanguage={targetLanguage}
                onSourceChange={setSourceLanguage}
                onTargetChange={setTargetLanguage}
                onSwap={handleSwap}
                t={t}
              />
              <ToneSelector
                selectedTone={tone}
                onSelectTone={setTone}
                t={t}
              />
            </View>

            <View style={styles.workspaceGrid}>
              <View style={styles.inputPane}>
                <TranslationInput
                  text={inputText}
                  onChangeText={(text) => {
                    setInputText(text);
                    if (error) setError(null);
                  }}
                  onClear={() => {
                    setInputText('');
                    setError(null);
                  }}
                  onSubmit={handleTranslate}
                  isLoading={isLoading}
                  error={error}
                  t={t}
                />
              </View>

              <View style={styles.divider} />

              <View style={styles.outputPane}>
                <TranslationOutput
                  result={result}
                  t={t}
                />
              </View>
            </View>

            <View style={styles.cardFooter}>
              <QuickPresets
                presets={presets}
                onSelectPreset={handleSelectPreset}
                t={t}
              />
            </View>
          </View>

          {result && (
            <ExplanationPanel
              result={result}
              t={t}
            />
          )}
        </View>

        <View style={styles.footer}>
          <Text style={styles.footerText}>{t.footerText}</Text>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.background,
  },
  scrollView: {
    flex: 1,
  },
  contentContainer: {
    paddingVertical: 24,
    paddingHorizontal: 16,
    maxWidth: 1040,
    width: '100%',
    alignSelf: 'center',
  },
  mainContainer: {
    gap: 16,
  },
  translatorCard: {
    backgroundColor: colors.surface,
    borderRadius: 14,
    borderWidth: 1,
    borderColor: colors.border,
    overflow: 'hidden',
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: 12,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    flexWrap: 'wrap',
    gap: 10,
    backgroundColor: 'rgba(19, 27, 46, 0.4)',
  },
  workspaceGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    minHeight: 220,
  },
  inputPane: {
    flex: 1,
    minWidth: 300,
  },
  divider: {
    width: 1,
    backgroundColor: colors.border,
  },
  outputPane: {
    flex: 1,
    minWidth: 300,
    backgroundColor: 'rgba(19, 27, 46, 0.25)',
  },
  cardFooter: {
    paddingHorizontal: 12,
    paddingVertical: 8,
    borderTopWidth: 1,
    borderTopColor: colors.border,
    backgroundColor: 'rgba(19, 27, 46, 0.4)',
  },
  footer: {
    marginTop: 32,
    paddingTop: 16,
    borderTopWidth: 1,
    borderTopColor: colors.border,
    alignItems: 'center',
  },
  footerText: {
    fontSize: 11,
    color: colors.textMuted,
  },
});
