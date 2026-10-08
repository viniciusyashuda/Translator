import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { colors } from '../theme/colors';

export function ChoicesSection({ choices, t }) {
  if (!choices || choices.length === 0) {
    return (
      <View style={styles.emptyBox}>
        <Text style={styles.emptyText}>Nenhuma escolha específica requer justificativa.</Text>
      </View>
    );
  }

  return (
    <View style={styles.list}>
      {choices.map((choice, index) => (
        <View key={`choice-${index}`} style={styles.itemCard}>
          <View style={styles.termFlow}>
            <Text style={styles.originalTerm}>"{choice.originalTerm}"</Text>
            <Text style={styles.arrow}>➔</Text>
            <Text style={styles.selectedTerm}>"{choice.selectedTerm}"</Text>
            {choice.alternativeTerm ? (
              <Text style={styles.rejectedTerm}>
                ({t.rejectedTerm}: {choice.alternativeTerm})
              </Text>
            ) : null}
          </View>

          <View style={styles.rationaleBox}>
            <Text style={styles.rationaleLabel}>{t.rationaleLabel}:</Text>
            <Text style={styles.rationaleText}>{choice.rationale}</Text>
          </View>
        </View>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  list: {
    gap: 10,
  },
  emptyBox: {
    padding: 16,
    alignItems: 'center',
  },
  emptyText: {
    fontSize: 13,
    color: colors.textMuted,
  },
  itemCard: {
    backgroundColor: colors.card,
    borderRadius: 8,
    padding: 12,
    borderWidth: 1,
    borderColor: colors.border,
    gap: 6,
  },
  termFlow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    flexWrap: 'wrap',
  },
  originalTerm: {
    fontSize: 13,
    color: colors.textPrimary,
    fontWeight: '600',
  },
  arrow: {
    color: colors.textMuted,
    fontSize: 12,
  },
  selectedTerm: {
    fontSize: 13,
    color: colors.accent,
    fontWeight: '700',
  },
  rejectedTerm: {
    fontSize: 11,
    color: colors.textMuted,
    textDecorationLine: 'line-through',
  },
  rationaleBox: {
    backgroundColor: colors.surface,
    padding: 8,
    borderRadius: 6,
  },
  rationaleLabel: {
    fontSize: 10,
    fontWeight: '600',
    color: colors.secondary,
    marginBottom: 2,
  },
  rationaleText: {
    fontSize: 12,
    color: colors.textSecondary,
    lineHeight: 17,
  },
});
