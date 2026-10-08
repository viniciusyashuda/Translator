import React from 'react';
import { View, Text, StyleSheet } from 'react-native';
import { colors } from '../theme/colors';

export function IdiomsSection({ items, t }) {
  if (!items || items.length === 0) {
    return (
      <View style={styles.emptyBox}>
        <Text style={styles.emptyText}>Nenhuma expressão idiomática detectada no texto.</Text>
      </View>
    );
  }

  return (
    <View style={styles.list}>
      {items.map((item, index) => (
        <View key={`idiom-${index}`} style={styles.itemCard}>
          <View style={styles.itemHeader}>
            <Text style={styles.expressionText}>"{item.expression}"</Text>
            {item.formality ? (
              <View style={styles.formalityTag}>
                <Text style={styles.formalityText}>{item.formality}</Text>
              </View>
            ) : null}
          </View>

          <View style={styles.meaningsRow}>
            <View style={styles.meaningCol}>
              <Text style={styles.meaningLabel}>{t.literalMeaning}</Text>
              <Text style={styles.literalText}>"{item.literalMeaning}"</Text>
            </View>

            <View style={styles.meaningCol}>
              <Text style={styles.meaningLabelHighlight}>{t.figurativeMeaning}</Text>
              <Text style={styles.figurativeText}>{item.figurativeMeaning}</Text>
            </View>
          </View>

          {item.explanation ? (
            <View style={styles.contextBox}>
              <Text style={styles.contextLabel}>{t.culturalContext}:</Text>
              <Text style={styles.contextText}>{item.explanation}</Text>
            </View>
          ) : null}
        </View>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  list: {
    gap: 12,
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
    gap: 8,
  },
  itemHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  expressionText: {
    fontSize: 14,
    fontWeight: '700',
    color: colors.textPrimary,
  },
  formalityTag: {
    backgroundColor: colors.surface,
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 4,
  },
  formalityText: {
    fontSize: 10,
    fontWeight: '600',
    color: colors.secondary,
  },
  meaningsRow: {
    flexDirection: 'row',
    gap: 12,
    flexWrap: 'wrap',
  },
  meaningCol: {
    flex: 1,
    minWidth: 180,
  },
  meaningLabel: {
    fontSize: 10,
    fontWeight: '600',
    color: colors.textMuted,
    textTransform: 'uppercase',
    marginBottom: 2,
  },
  meaningLabelHighlight: {
    fontSize: 10,
    fontWeight: '600',
    color: colors.accent,
    textTransform: 'uppercase',
    marginBottom: 2,
  },
  literalText: {
    fontSize: 12,
    color: colors.textSecondary,
    fontStyle: 'italic',
  },
  figurativeText: {
    fontSize: 12,
    color: colors.textPrimary,
    fontWeight: '500',
  },
  contextBox: {
    backgroundColor: colors.surface,
    padding: 8,
    borderRadius: 6,
  },
  contextLabel: {
    fontSize: 10,
    fontWeight: '600',
    color: colors.secondary,
    marginBottom: 2,
  },
  contextText: {
    fontSize: 12,
    color: colors.textSecondary,
    lineHeight: 17,
  },
});
