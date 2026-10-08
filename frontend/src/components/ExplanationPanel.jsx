import React, { useState } from 'react';
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { colors } from '../theme/colors';
import { IdiomsSection } from './IdiomsSection';
import { ChoicesSection } from './ChoicesSection';
import { AlternativesSection } from './AlternativesSection';

export function ExplanationPanel({ result, t }) {
  const [activeTab, setActiveTab] = useState('idioms');

  if (!result) return null;

  const idioms = result.idiomsAndNuances || [];
  const choices = result.translationChoices || [];
  const alternatives = result.alternativePhrasings || result.AlternativePhrasings || [];

  return (
    <View style={styles.container}>
      {result.summaryExplanation ? (
        <View style={styles.summaryBar}>
          <Text style={styles.summaryIcon}>💡</Text>
          <Text style={styles.summaryText}>{result.summaryExplanation}</Text>
        </View>
      ) : null}

      <View style={styles.tabBar}>
        <TouchableOpacity
          style={[styles.tabButton, activeTab === 'idioms' && styles.tabButtonActive]}
          onPress={() => setActiveTab('idioms')}
        >
          <Text style={[styles.tabText, activeTab === 'idioms' && styles.tabTextActive]}>
            {t.tabIdioms}
          </Text>
          {idioms.length > 0 && (
            <View style={[styles.badge, activeTab === 'idioms' && styles.badgeActive]}>
              <Text style={styles.badgeText}>{idioms.length}</Text>
            </View>
          )}
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.tabButton, activeTab === 'choices' && styles.tabButtonActive]}
          onPress={() => setActiveTab('choices')}
        >
          <Text style={[styles.tabText, activeTab === 'choices' && styles.tabTextActive]}>
            {t.tabChoices}
          </Text>
          {choices.length > 0 && (
            <View style={[styles.badge, activeTab === 'choices' && styles.badgeActive]}>
              <Text style={styles.badgeText}>{choices.length}</Text>
            </View>
          )}
        </TouchableOpacity>

        <TouchableOpacity
          style={[styles.tabButton, activeTab === 'alternatives' && styles.tabButtonActive]}
          onPress={() => setActiveTab('alternatives')}
        >
          <Text style={[styles.tabText, activeTab === 'alternatives' && styles.tabTextActive]}>
            {t.tabAlternatives}
          </Text>
          {alternatives.length > 0 && (
            <View style={[styles.badge, activeTab === 'alternatives' && styles.badgeActive]}>
              <Text style={styles.badgeText}>{alternatives.length}</Text>
            </View>
          )}
        </TouchableOpacity>
      </View>

      <View style={styles.tabContent}>
        {activeTab === 'idioms' && <IdiomsSection items={idioms} t={t} />}
        {activeTab === 'choices' && <ChoicesSection choices={choices} t={t} />}
        {activeTab === 'alternatives' && <AlternativesSection alternatives={alternatives} t={t} />}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    backgroundColor: colors.surface,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: colors.border,
    padding: 16,
    gap: 12,
  },
  summaryBar: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    gap: 8,
    padding: 10,
    borderRadius: 8,
    backgroundColor: 'rgba(99, 102, 241, 0.08)',
  },
  summaryIcon: {
    fontSize: 14,
  },
  summaryText: {
    fontSize: 13,
    color: '#D1D5DB',
    lineHeight: 18,
    flex: 1,
  },
  tabBar: {
    flexDirection: 'row',
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
    gap: 4,
  },
  tabButton: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    paddingVertical: 8,
    paddingHorizontal: 12,
    borderBottomWidth: 2,
    borderBottomColor: 'transparent',
  },
  tabButtonActive: {
    borderBottomColor: colors.primary,
  },
  tabText: {
    fontSize: 12,
    fontWeight: '600',
    color: colors.textMuted,
  },
  tabTextActive: {
    color: colors.textPrimary,
    fontWeight: '700',
  },
  badge: {
    backgroundColor: colors.card,
    paddingHorizontal: 5,
    paddingVertical: 1,
    borderRadius: 8,
  },
  badgeActive: {
    backgroundColor: colors.primaryDark,
  },
  badgeText: {
    fontSize: 10,
    fontWeight: '700',
    color: colors.textPrimary,
  },
  tabContent: {
    paddingTop: 4,
  },
});
