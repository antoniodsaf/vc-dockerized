<template>
  <VcPopup>
    <iframe :src="props.iframeURL" title="cpq-config-iframes"></iframe>
  </VcPopup>
</template>
<script setup lang="ts">
interface IProps {
  iframeURL: string;
}
const props = defineProps<IProps>();
</script>
