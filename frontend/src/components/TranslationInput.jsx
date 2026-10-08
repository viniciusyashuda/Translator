import React from 'react';
import {
  View,
  Text,
  TextInput,
  TouchableOpacity,
  ActivityIndicator,
  StyleSheet,
} from 'react-native';
import { colors } from '../theme/colors';

export function TranslationInput({
  text,
  onChangeText,
  onClear,
  onSubmit,
  isLoading,
  error,
  t,
}) {
  const charCount = text.length;
  const maxChars = 2000;

  return (
    <View style={styles.container}>
      <TextInput
        style={styles.input}
        multiline
        numberOfLines={5}
        value={text}
        onChangeText={onChangeText}
        placeholder={t.inputPlaceholder}
        placeholderTextColor={colors.textMuted}
        maxLength={maxChars}
      />

      {error ? (
        <View style={styles.errorBox}>
          <Text style={styles.errorText}>⚠️ {error}</Text>
        </View>
      ) : null}

      <View style={styles.footerRow}>
        <View style={styles.leftMeta}>
          <Text style={styles.counter}>
            {charCount}/{maxChars}
          </Text>
          {text.length > 0 && (
            <TouchableOpacity onPress={onClear} style={styles.clearBtn}>
              <Text style={styles.clearBtnText}>{t.clearButton}</Text>
            </TouchableOpacity>
          )}
        </View>

        <TouchableOpacity
          style={[styles.submitBtn, (!text.trim() || isLoading) && styles.submitBtnDisabled]}
          onPress={onSubmit}
          disabled={!text.trim() || isLoading}
        >
          {isLoading ? (
            <View style={styles.loadingRow}>
              <ActivityIndicator size="small" color="#FFFFFF" />
              <Text style={styles.submitBtnText}>{t.translatingButton}</Text>
            </View>
          ) : (
            <Text style={styles.submitBtnText}>{t.translateButton}</Text>
          )}
        </TouchableOpacity>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    minHeight: 180,
    justifyContent: 'space-between',
  },
  input: {
    color: colors.textPrimary,
    fontSize: 16,
    lineHeight: 24,
    padding: 12,
    minHeight: 120,
    textAlignVertical: 'top',
    outlineStyle: 'none',
  },
  errorBox: {
    backgroundColor: colors.dangerSoft,
    padding: 8,
    borderRadius: 6,
    marginHorizontal: 12,
    marginBottom: 8,
  },
  errorText: {
    color: '#FCA5A5',
    fontSize: 12,
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
  leftMeta: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 10,
  },
  counter: {
    fontSize: 11,
    color: colors.textMuted,
    fontFamily: 'monospace',
  },
  clearBtn: {
    paddingVertical: 2,
    paddingHorizontal: 6,
    borderRadius: 4,
    backgroundColor: colors.card,
  },
  clearBtnText: {
    fontSize: 11,
    color: colors.textSecondary,
  },
  submitBtn: {
    backgroundColor: colors.primary,
    paddingVertical: 8,
    paddingHorizontal: 16,
    borderRadius: 8,
  },
  submitBtnDisabled: {
    opacity: 0.4,
  },
  submitBtnText: {
    color: '#FFFFFF',
    fontSize: 13,
    fontWeight: '700',
  },
  loadingRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 6,
  },
});
