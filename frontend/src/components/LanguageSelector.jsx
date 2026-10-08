import React from 'react';
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { colors } from '../theme/colors';

export function LanguageSelector({
  languages,
  sourceLanguage,
  targetLanguage,
  onSourceChange,
  onTargetChange,
  onSwap,
  t,
}) {
  const getLanguageLabel = (code) => {
    return t.languages[code] || code.toUpperCase();
  };

  return (
    <View style={styles.container}>
      <View style={styles.selectorWrapper}>
        <select
          value={sourceLanguage}
          onChange={(e) => onSourceChange(e.target.value)}
          style={selectStyle}
        >
          {languages.map((lang) => (
            <option key={`src-${lang.code}`} value={lang.code} style={optionStyle}>
              {lang.flag} {getLanguageLabel(lang.code)}
            </option>
          ))}
        </select>
      </View>

      <TouchableOpacity
        style={[styles.swapButton, sourceLanguage === 'auto' && styles.swapButtonDisabled]}
        onPress={onSwap}
        disabled={sourceLanguage === 'auto'}
        title="Inverter idiomas"
      >
        <Text style={styles.swapIcon}>⇄</Text>
      </TouchableOpacity>

      <View style={styles.selectorWrapper}>
        <select
          value={targetLanguage}
          onChange={(e) => onTargetChange(e.target.value)}
          style={selectStyle}
        >
          {languages
            .filter((lang) => lang.code !== 'auto')
            .map((lang) => (
              <option key={`tgt-${lang.code}`} value={lang.code} style={optionStyle}>
                {lang.flag} {getLanguageLabel(lang.code)}
              </option>
            ))}
        </select>
      </View>
    </View>
  );
}

const selectStyle = {
  backgroundColor: colors.card,
  color: colors.textPrimary,
  border: `1px solid ${colors.border}`,
  borderRadius: 8,
  padding: '8px 12px',
  fontSize: 13,
  fontWeight: '600',
  fontFamily: 'inherit',
  cursor: 'pointer',
  outline: 'none',
  width: '100%',
};

const optionStyle = {
  backgroundColor: colors.surface,
  color: colors.textPrimary,
  padding: '8px',
};

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
    flexWrap: 'wrap',
  },
  selectorWrapper: {
    flex: 1,
    minWidth: 140,
  },
  swapButton: {
    width: 34,
    height: 34,
    borderRadius: 8,
    backgroundColor: colors.card,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
    justifyContent: 'center',
  },
  swapButtonDisabled: {
    opacity: 0.4,
  },
  swapIcon: {
    fontSize: 16,
    color: colors.textSecondary,
    fontWeight: 'bold',
  },
});
