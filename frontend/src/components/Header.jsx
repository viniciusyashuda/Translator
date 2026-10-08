import React from 'react';
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { colors } from '../theme/colors';

export function Header({ backendStatus, uiLanguage, onToggleUiLanguage, t }) {
  const isHealthy = backendStatus?.healthy;

  return (
    <View style={styles.container}>
      <View style={styles.branding}>
        <Text style={styles.logoText}>Translator</Text>
        <View style={styles.aiBadge}>
          <Text style={styles.aiBadgeText}>{t.aiBadge}</Text>
        </View>
      </View>

      <View style={styles.controls}>
        <View style={styles.langToggle}>
          <TouchableOpacity
            style={[styles.langOption, uiLanguage === 'pt' && styles.langOptionActive]}
            onPress={() => onToggleUiLanguage('pt')}
          >
            <Text style={[styles.langText, uiLanguage === 'pt' && styles.langTextActive]}>
              PT-BR
            </Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.langOption, uiLanguage === 'en' && styles.langOptionActive]}
            onPress={() => onToggleUiLanguage('en')}
          >
            <Text style={[styles.langText, uiLanguage === 'en' && styles.langTextActive]}>
              EN-US
            </Text>
          </TouchableOpacity>
        </View>

        <View style={styles.statusPill}>
          <View
            style={[
              styles.statusDot,
              {
                backgroundColor: !isHealthy
                  ? colors.danger
                  : backendStatus?.apiKeyConfigured
                  ? colors.accent
                  : colors.warning,
              },
            ]}
          />
          <Text style={styles.statusText}>
            {!isHealthy
              ? t.statusConnecting
              : backendStatus?.apiKeyConfigured
              ? t.keyActive
              : t.keyMissing}
          </Text>
        </View>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: 14,
    paddingHorizontal: 24,
    backgroundColor: colors.surface,
    borderBottomWidth: 1,
    borderBottomColor: colors.border,
  },
  branding: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  logoText: {
    fontSize: 18,
    fontWeight: '700',
    color: colors.textPrimary,
    letterSpacing: -0.3,
  },
  aiBadge: {
    backgroundColor: colors.primaryDark,
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 4,
  },
  aiBadgeText: {
    color: '#EEF2FF',
    fontSize: 10,
    fontWeight: '700',
  },
  controls: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 12,
  },
  langToggle: {
    flexDirection: 'row',
    backgroundColor: colors.card,
    borderRadius: 8,
    padding: 2,
    borderWidth: 1,
    borderColor: colors.border,
  },
  langOption: {
    paddingHorizontal: 8,
    paddingVertical: 4,
    borderRadius: 6,
  },
  langOptionActive: {
    backgroundColor: colors.primary,
  },
  langText: {
    fontSize: 11,
    fontWeight: '600',
    color: colors.textSecondary,
  },
  langTextActive: {
    color: '#FFFFFF',
    fontWeight: '700',
  },
  statusPill: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
    paddingHorizontal: 8,
    paddingVertical: 4,
    backgroundColor: colors.card,
    borderRadius: 8,
    borderWidth: 1,
    borderColor: colors.border,
  },
  statusDot: {
    width: 6,
    height: 6,
    borderRadius: 3,
  },
  statusText: {
    fontSize: 11,
    color: colors.textSecondary,
  },
});
