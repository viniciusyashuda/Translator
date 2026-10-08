import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { colors } from '../theme/colors';

export function AlternativesSection({ alternatives, t }) {
  if (!alternatives || alternatives.length === 0) {
    return (
      <View style={styles.emptyBox}>
        <Text style={styles.emptyText}>Nenhuma variação alternativa disponível.</Text>
      </View>
    );
  }

  return (
    <View style={styles.grid}>
      {alternatives.map((alt, index) => (
        <View key={`alt-${index}`} style={styles.itemCard}>
          <View style={styles.header}>
            <View style={styles.toneTag}>
              <Text style={styles.toneTagText}>{alt.tone}</Text>
            </View>
          </View>

          <Text style={styles.translationText} selectable>
            "{alt.translation}"
          </Text>

          {alt.context ? (
            <Text style={styles.contextText}>
              <Text style={styles.contextLabel}>{t.recommendedContext}: </Text>
              {alt.context}
            </Text>
          ) : null}
        </View>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  grid: {
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
  header: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  toneTag: {
    backgroundColor: colors.surface,
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 4,
  },
  toneTagText: {
    fontSize: 10,
    fontWeight: '700',
    color: colors.primaryLight,
  },
  translationText: {
    fontSize: 13,
    color: colors.textPrimary,
    fontWeight: '500',
    lineHeight: 18,
  },
  contextText: {
    fontSize: 11,
    color: colors.textSecondary,
    lineHeight: 16,
  },
  contextLabel: {
    fontWeight: '600',
    color: colors.textMuted,
  },
});
