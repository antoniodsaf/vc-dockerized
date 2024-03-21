<template>
  <VcPopup class="ps-4" modal-width="auto" @close="onClose">
    <div class="flex justify-center items-center">
      <iframe
        id="virto-poc-iframe"
        :src="props.iframeURL"
        title="cpq-config-iframes"
        class="w-[90vw] h-[100vh]"
      ></iframe>
    </div>
  </VcPopup>
</template>
<script setup lang="ts">
interface IProps {
  iframeURL: string;
  /* eslint-disable  @typescript-eslint/no-explicit-any */
  listener: any;
}
const props = defineProps<IProps>();
const onClose = () => {
  console.log("::: listener removed");
  window.removeEventListener("message", props.listener);
};
</script>
