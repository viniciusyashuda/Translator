import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { colors } from '../theme/colors';

const TONE_KEYS = ['Natural', 'Casual', 'Formal', 'Business', 'Colloquial', 'Academic'];

export function ToneSelector({ selectedTone, onSelectTone, t }) {
  return (
    <View style={styles.container}>
      <Text style={styles.label}>{t.toneLabel}:</Text>
      <select
        value={selectedTone}
        onChange={(e) => onSelectTone(e.target.value)}
        style={selectStyle}
      >
        {TONE_KEYS.map((key) => (
          <option key={key} value={key} style={optionStyle}>
            {t.tones[key] || key}
          </option>
        ))}
      </select>
    </View>
  );
}

const selectStyle = {
  backgroundColor: colors.card,
  color: colors.textPrimary,
  border: `1px solid ${colors.border}`,
  borderRadius: 8,
  padding: '6px 10px',
  fontSize: 12,
  fontWeight: '600',
  fontFamily: 'inherit',
  cursor: 'pointer',
  outline: 'none',
};

const optionStyle = {
  backgroundColor: colors.surface,
  color: colors.textPrimary,
  padding: '6px',
};

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  label: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.textSecondary,
  },
});
