import React, { useState } from 'react';
import { View, Text, TouchableOpacity, StyleSheet } from 'react-native';
import { colors } from '../theme/colors';

export function TranslationOutput({ result, t }) {
  const [copied, setCopied] = useState(false);

  const handleCopy = () => {
    if (result?.translatedText && typeof navigator !== 'undefined' && navigator.clipboard) {
      navigator.clipboard.writeText(result.translatedText);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  const handleSpeak = () => {
    if (result?.translatedText && typeof window !== 'undefined' && 'speechSynthesis' in window) {
      window.speechSynthesis.cancel();
      const utterance = new SpeechSynthesisUtterance(result.translatedText);
      utterance.lang = result.targetLanguage === 'pt' ? 'pt-BR' : result.targetLanguage;
      window.speechSynthesis.speak(utterance);
    }
  };

  if (!result) {
    return (
      <View style={styles.emptyContainer}>
        <Text style={styles.placeholderText}>
          {t.emptyTitle}
        </Text>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.textContainer}>
        <Text style={styles.translatedText} selectable>
          {result.translatedText}
        </Text>
      </View>

      <View style={styles.footerRow}>
        <View style={styles.metaRow}>
          {result.detectedSourceLanguage ? (
            <View style={styles.metaPill}>
              <Text style={styles.metaText}>
                {t.detectedLabel}: {result.detectedSourceLanguage.toUpperCase()}
              </Text>
            </View>
          ) : null}
          {result.isCached ? (
            <View style={styles.cachePill}>
              <Text style={styles.cacheText}>⚡ {t.cachedLabel}</Text>
            </View>
          ) : null}
        </View>

        <View style={styles.actionsRow}>
          <TouchableOpacity
            style={styles.actionBtn}
            onPress={handleSpeak}
            title={t.speakButton}
          >
            <Text style={styles.actionIcon}>🔊</Text>
          </TouchableOpacity>

          <TouchableOpacity
            style={[styles.actionBtn, copied && styles.actionBtnActive]}
            onPress={handleCopy}
            title={t.copyButton}
          >
            <Text style={styles.actionText}>{copied ? t.copiedButton : t.copyButton}</Text>
          </TouchableOpacity>
        </View>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  emptyContainer: {
    flex: 1,
    minHeight: 180,
    alignItems: 'center',
    justifyContent: 'center',
    padding: 24,
  },
  placeholderText: {
    color: colors.textMuted,
    fontSize: 14,
    textAlign: 'center',
  },
  container: {
    flex: 1,
    minHeight: 180,
    justifyContent: 'space-between',
  },
  textContainer: {
    padding: 12,
    minHeight: 120,
  },
  translatedText: {
    color: colors.textPrimary,
    fontSize: 16,
    lineHeight: 24,
    fontWeight: '500',
  },
  footerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 12,
    paddingVertical: 10,
    borderTopWidth: 1,
    borderTopColor: colors.border,
  },
  metaRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
  },
  metaPill: {
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 4,
    backgroundColor: colors.card,
  },
  metaText: {
    fontSize: 10,
    color: colors.textSecondary,
    fontWeight: '600',
  },
  cachePill: {
    paddingHorizontal: 6,
    paddingVertical: 2,
    borderRadius: 4,
    backgroundColor: colors.accentSoft,
  },
  cacheText: {
    fontSize: 10,
    color: colors.accent,
    fontWeight: '600',
  },
  actionsRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 8,
  },
  actionBtn: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 4,
    paddingVertical: 4,
    paddingHorizontal: 8,
    borderRadius: 6,
    backgroundColor: colors.card,
  },
  actionBtnActive: {
    backgroundColor: colors.primaryDark,
  },
  actionIcon: {
    fontSize: 13,
  },
  actionText: {
    fontSize: 11,
    fontWeight: '600',
    color: colors.textSecondary,
  },
});
