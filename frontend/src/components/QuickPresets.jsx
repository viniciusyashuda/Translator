import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { colors } from '../theme/colors';

export function QuickPresets({ presets, onSelectPreset, t }) {
  if (!presets || presets.length === 0) {
    return null;
  }

  const handleChange = (e) => {
    const selectedId = e.target.value;
    if (!selectedId) return;
    const found = presets.find((p) => p.id === selectedId);
    if (found) {
      onSelectPreset(found);
    }
  };

  return (
    <View style={styles.container}>
      <Text style={styles.label}>💡 {t.quickExamples}:</Text>
      <select
        onChange={handleChange}
        defaultValue=""
        style={selectStyle}
      >
        <option value="" disabled style={optionStyle}>
          {t.selectExample}
        </option>
        {presets.map((preset) => (
          <option key={preset.id} value={preset.id} style={optionStyle}>
            [{preset.sourceLanguage.toUpperCase()} ➔ {preset.targetLanguage.toUpperCase()}] {preset.text}
          </option>
        ))}
      </select>
    </View>
  );
}

const selectStyle = {
  backgroundColor: 'rgba(99, 102, 241, 0.08)',
  color: colors.primaryLight,
  border: `1px solid ${colors.border}`,
  borderRadius: 8,
  padding: '6px 10px',
  fontSize: 12,
  fontWeight: '500',
  fontFamily: 'inherit',
  cursor: 'pointer',
  outline: 'none',
  maxWidth: 340,
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
    flexWrap: 'wrap',
  },
  label: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.textSecondary,
  },
});
